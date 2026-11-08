using KampConnect.Identity.Business.Contracts.Dto;
using KampConnect.Identity.Data.Entity.Roles;
using KampConnect.Identity.Data.Entity.Tenant.Invitations;

namespace KampConnect.Identity.Business.Logic.Helpers;

/// <summary>
/// Gedeelde regels voor uitnodigingen, gebruikt door <c>InvitationService</c> (bestaande gebruiker)
/// én <c>AccountService</c> (registratie met uitnodigingstoken).
/// </summary>
internal static class InvitationRules
{
    /// <summary>Kan de uitnodiging nog aanvaard of geweigerd worden?</summary>
    public static bool IsUsable(TenantInvitation invitation, DateTime now) =>
        invitation.status == InvitationStatus.Pending && invitation.expiry_timestamp > now;

    /// <summary>Hoort de uitnodiging bij dit (genormaliseerd) e-mailadres?</summary>
    public static bool MatchesEmail(TenantInvitation invitation, string? normalizedEmail) =>
        !string.IsNullOrEmpty(normalizedEmail) &&
        EmailNormalizer.Normalize(invitation.email) == normalizedEmail;

    /// <summary>Zet een vervallen Pending-uitnodiging op Expired. Geeft true als dat gebeurde.</summary>
    public static bool ExpireIfNeeded(TenantInvitation invitation, DateTime now)
    {
        if (invitation.status != InvitationStatus.Pending || invitation.expiry_timestamp > now) return false;
        invitation.status = InvitationStatus.Expired;
        invitation.modification_timestamp = now;
        return true;
    }

    /// <summary>Markeert de uitnodiging als aanvaard en maakt de koppeling gebruiker ↔ rol ↔ tenant.</summary>
    public static UserRole Accept(TenantInvitation invitation, Guid userId, DateTime now)
    {
        invitation.status = InvitationStatus.Accepted;
        invitation.accepted_by_user_id = userId;
        invitation.accepted_timestamp = now;
        invitation.modification_timestamp = now;

        return new UserRole
        {
            user_id = userId,
            role_id = invitation.role_id,
            tenant_id = invitation.tenant_id,
            creation_timestamp = now
        };
    }

    public static InvitationDto ToDto(TenantInvitation invitation) => new()
    {
        TenantInvitationId = invitation.tenant_invitation_id,
        TenantId = invitation.tenant_id,
        TenantName = invitation.TenantSnapshot?.tenant_name,
        Email = invitation.email,
        RoleId = invitation.role_id,
        RoleCode = invitation.Role?.role_code,
        RoleName = invitation.Role?.role_name,
        Status = invitation.status,
        SentByUserId = invitation.sent_by_user_id,
        AcceptedByUserId = invitation.accepted_by_user_id,
        CancelledByUserId = invitation.cancelled_by_user_id,
        ExpiryTimestamp = invitation.expiry_timestamp,
        CreatedTimestamp = invitation.creation_timestamp,
        AcceptedTimestamp = invitation.accepted_timestamp,
        CancelledTimestamp = invitation.cancelled_timestamp
    };
}
