using System.Net;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Contracts.Requests;
using KampConnect.Identity.Business.Logic.Helpers;
using KampConnect.Identity.Data.Entity.Tenant.Invitations;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KampConnect.Identity.Business.Logic.Implementation;

/// <summary>
/// Uitnodigingen om lid te worden van een tenant.
/// <para>
/// Het token gaat enkel in de mail; in de database staat de hash (zie <see cref="SecureToken"/>).
/// Aanvaarden kan op twee manieren:
/// <list type="bullet">
/// <item>bestaande gebruiker: ingelogd → <see cref="AcceptInvitationAsync"/>;</item>
/// <item>nieuwe gebruiker: registreren met het token → <see cref="AccountService.RegisterAsync"/>.</item>
/// </list>
/// </para>
/// </summary>
public class InvitationService(
    ITenantInvitationRepository tenantInvitationRepository,
    ITenantSnapshotRepository tenantSnapshotRepository,
    IUserRoleRepository userRoleRepository,
    IUserRepository userRepository,
    IIdentityMailSender mailSender,
    IOptions<KampConnectIdentitySettings> options,
    ILogger<InvitationService> logger) : IInvitationService
{
    private readonly KampConnectIdentitySettings _settings = options.Value;

    #region Create

    public async Task<InvitationResult> CreateInvitationAsync(Guid sentByUserId, int tenantId, CreateInvitationRequest request, CancellationToken ct = default)
    {
        // 1. Invoer controleren
        var validator = new AccountValidator().Email("email", request.Email);
        if (!validator.IsValid) return InvitationResult.Fail(validator.Errors);

        var email = request.Email.Trim().ToLowerInvariant();

        // 2. Tenant en rol moeten bestaan; platformrollen kan je niet via een tenant uitdelen
        var tenants = await tenantSnapshotRepository.GetTenantsByIdAsync([tenantId], ct);
        if (tenants.Count == 0)
            return InvitationResult.Fail(InvitationError.NotFound, "De opgegeven tenant bestaat niet.");

        var role = await userRoleRepository.GetRoleByIdAsync(request.RoleId, ct);
        if (role is null || role.is_platform_role)
            return InvitationResult.Fail(new Dictionary<string, string[]> { { "roleId", ["Ongeldige rol."] } });

        // 3. Heeft deze persoon de rol al binnen de tenant?
        var existingUser = await userRepository.GetByNormalizedEmailAsync(EmailNormalizer.Normalize(email), ct);
        if (existingUser is not null && await userRoleRepository.RoleExistsAsync(existingUser.user_id, role.role_id, tenantId, ct))
            return InvitationResult.Fail(InvitationError.Conflict, "Deze gebruiker heeft deze rol al binnen de organisatie.");

        // 4. Al een openstaande uitnodiging? Vervallen → op Expired zetten zodat er een nieuwe kan komen.
        var now = DateTime.UtcNow;
        var pending = await tenantInvitationRepository.GetPendingInvitationAsync(tenantId, email, ct);
        if (pending is not null && !InvitationRules.ExpireIfNeeded(pending, now))
            return InvitationResult.Fail(InvitationError.Conflict, "Er bestaat al een openstaande uitnodiging voor dit e-mailadres.");

        // 5. Uitnodiging aanmaken
        var (token, hash) = SecureToken.Create();
        var invitation = new TenantInvitation
        {
            tenant_id = tenantId,
            email = email,
            role_id = role.role_id,
            sent_by_user_id = sentByUserId,
            status = InvitationStatus.Pending,
            token_hash = hash,
            creation_timestamp = now,
            expiry_timestamp = now.AddDays(_settings.InvitationLifetimeDays),
            TenantSnapshot = tenants[0],
            Role = role
        };

        tenantInvitationRepository.Add(invitation);
        await userRepository.SaveChangesAsync(ct);

        // 6. Mail versturen (na het bewaren: het token moet bestaan als de gebruiker klikt)
        await SendInvitationMailAsync(invitation, token, ct);

        logger.LogInformation("Uitnodiging {InvitationId} voor tenant {TenantId} aangemaakt door {UserId}",
            invitation.tenant_invitation_id, tenantId, sentByUserId);
        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    #endregion

    #region Read

    public async Task<InvitationResult> GetInvitationsAsync(int tenantId, CancellationToken ct = default)
    {
        var invitations = await tenantInvitationRepository.GetInvitationsByTenantIdAsync(tenantId, ct);
        return InvitationResult.Success(invitations.Select(InvitationRules.ToDto).ToList());
    }

    public async Task<InvitationResult> GetInvitationByTokenAsync(string token, CancellationToken ct = default)
    {
        var invitation = await FindByTokenAsync(token, ct);
        if (invitation is null)
            return InvitationResult.Fail(InvitationError.InvalidToken, "Uitnodiging ongeldig of verlopen.");

        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    #endregion

    #region Cancel / resend (tenant-admin)

    public async Task<InvitationResult> CancelInvitationAsync(Guid cancelledByUserId, int tenantId, int invitationId, CancellationToken ct = default)
    {
        var invitation = await tenantInvitationRepository.GetInvitationByIdAsync(invitationId, ct);
        if (invitation is null || invitation.tenant_id != tenantId)
            return InvitationResult.Fail(InvitationError.NotFound, "Uitnodiging niet gevonden.");

        if (invitation.status != InvitationStatus.Pending)
            return InvitationResult.Fail(InvitationError.Conflict, "Enkel openstaande uitnodigingen kunnen ingetrokken worden.");

        var now = DateTime.UtcNow;
        invitation.status = InvitationStatus.Cancelled;
        invitation.cancelled_by_user_id = cancelledByUserId;
        invitation.cancelled_timestamp = now;
        invitation.modification_timestamp = now;
        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("Uitnodiging {InvitationId} ingetrokken door {UserId}", invitationId, cancelledByUserId);
        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    public async Task<InvitationResult> ResendInvitationAsync(Guid userId, int tenantId, int invitationId, CancellationToken ct = default)
    {
        var invitation = await tenantInvitationRepository.GetInvitationByIdAsync(invitationId, ct);
        if (invitation is null || invitation.tenant_id != tenantId)
            return InvitationResult.Fail(InvitationError.NotFound, "Uitnodiging niet gevonden.");

        // Ook een vervallen (nog Pending) uitnodiging mag opnieuw verstuurd worden.
        if (invitation.status != InvitationStatus.Pending)
            return InvitationResult.Fail(InvitationError.Conflict, "Enkel openstaande uitnodigingen kunnen opnieuw verstuurd worden.");

        // Nieuw token: de link uit de vorige mail werkt niet meer.
        var now = DateTime.UtcNow;
        var (token, hash) = SecureToken.Create();
        invitation.token_hash = hash;
        invitation.expiry_timestamp = now.AddDays(_settings.InvitationLifetimeDays);
        invitation.modification_timestamp = now;
        await userRepository.SaveChangesAsync(ct);

        await SendInvitationMailAsync(invitation, token, ct);

        logger.LogInformation("Uitnodiging {InvitationId} opnieuw verstuurd door {UserId}", invitationId, userId);
        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    #endregion

    #region Accept / decline (uitgenodigde persoon)

    public async Task<InvitationResult> AcceptInvitationAsync(Guid userId, AcceptInvitationRequest request, CancellationToken ct = default)
    {
        var invitation = await FindByTokenAsync(request.Token, ct);
        if (invitation is null)
            return InvitationResult.Fail(InvitationError.InvalidToken, "Uitnodiging ongeldig of verlopen.");

        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null || !user.is_active)
            return InvitationResult.Fail(InvitationError.InvalidToken, "Uitnodiging ongeldig of verlopen.");

        // De uitnodiging is persoonlijk: enkel het uitgenodigde e-mailadres mag ze aanvaarden.
        if (!InvitationRules.MatchesEmail(invitation, user.email_normalized))
            return InvitationResult.Fail(InvitationError.EmailMismatch, "Deze uitnodiging is voor een ander e-mailadres.");

        var now = DateTime.UtcNow;
        var userRole = InvitationRules.Accept(invitation, user.user_id, now);

        // Rol al aanwezig (bv. via een andere weg)? Dan enkel de uitnodiging afsluiten.
        if (!await userRoleRepository.RoleExistsAsync(user.user_id, invitation.role_id, invitation.tenant_id, ct))
        {
            userRoleRepository.Add(userRole);
        }

        // Wie de mail kon openen, bewijst ook dat het e-mailadres van hem is.
        user.email_verified_timestamp ??= now;
        user.modification_timestamp = now;

        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("Uitnodiging {InvitationId} aanvaard door {UserId}", invitation.tenant_invitation_id, user.user_id);
        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    public async Task<InvitationResult> DeclineInvitationAsync(DeclineInvitationRequest request, CancellationToken ct = default)
    {
        var invitation = await FindByTokenAsync(request.Token, ct);
        if (invitation is null)
            return InvitationResult.Fail(InvitationError.InvalidToken, "Uitnodiging ongeldig of verlopen.");

        var now = DateTime.UtcNow;
        invitation.status = InvitationStatus.Declined;
        invitation.modification_timestamp = now;
        await userRepository.SaveChangesAsync(ct);

        logger.LogInformation("Uitnodiging {InvitationId} geweigerd", invitation.tenant_invitation_id);
        return InvitationResult.Success(InvitationRules.ToDto(invitation));
    }

    #endregion

    #region Helpers

    /// <summary>Bruikbare (Pending, niet vervallen) uitnodiging via het token, anders null.</summary>
    private async Task<TenantInvitation?> FindByTokenAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var invitation = await tenantInvitationRepository.GetInvitationByTokenHashAsync(SecureToken.Hash(token), ct);
        if (invitation is null) return null;

        if (InvitationRules.ExpireIfNeeded(invitation, DateTime.UtcNow))
        {
            await userRepository.SaveChangesAsync(ct);
            return null;
        }

        return InvitationRules.IsUsable(invitation, DateTime.UtcNow) ? invitation : null;
    }

    private Task SendInvitationMailAsync(TenantInvitation invitation, string token, CancellationToken ct)
    {
        // TODO: Mailservice: IIdentityMailSender is nu nog LoggingMailSender (mail wordt enkel gelogd).
        //       Zodra er een SMTP-implementatie is, wordt deze uitnodiging echt verstuurd.
        var link = string.Format(_settings.InvitationUrl, Uri.EscapeDataString(token));
        var tenantName = WebUtility.HtmlEncode(invitation.TenantSnapshot?.tenant_name ?? "een organisatie");
        var roleName = WebUtility.HtmlEncode(invitation.Role?.role_name ?? "lid");

        return mailSender.SendAsync(invitation.email, $"KampConnect – uitnodiging voor {invitation.TenantSnapshot?.tenant_name ?? "een organisatie"}",
            $"<p>Je bent uitgenodigd om als <b>{roleName}</b> deel te nemen aan <b>{tenantName}</b> op KampConnect.</p>" +
            $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Bekijk de uitnodiging</a> " +
            $"(geldig gedurende {_settings.InvitationLifetimeDays} dagen).</p>" +
            $"<p>Ken je deze organisatie niet? Dan mag je deze mail negeren.</p>", ct);
    }

    #endregion
}
