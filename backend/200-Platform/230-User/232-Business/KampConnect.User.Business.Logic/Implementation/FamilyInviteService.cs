using KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;
using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Logic.Helpers;
using KampConnect.User.Business.Logic.Mapping;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.Extensions.Logging;

namespace KampConnect.User.Business.Logic.Implementation;

public class FamilyInviteService(
    IFamilyRepository familyRepository,
    IFamilyInviteRepository inviteRepository,
    IUserRepository userRepository,
    TimeProvider timeProvider,
    ILogger<FamilyInviteService> logger) : IFamilyInviteService
{
    #region CreateInviteAsync
    public async Task<FamilyResult> CreateInviteAsync(Guid familyId, Guid authUserId, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        if (!access.Family.invite_enabled)
        {
            return FamilyResult.Fail(FamilyError.InvitesDisabled);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var code = InviteCodeHelper.Generate();

        // Enkel de hash wordt opgeslagen, de code zelf wordt maar één keer teruggegeven
        var invite = new UserFamilyInvite
        {
            family_id = familyId,
            code_hash = InviteCodeHelper.Hash(code),
            created_by_user_id = access.User.user_id,
            expires_at = now.Add(InviteCodeHelper.Lifetime),
            used_count = 0,
            created_at = now
        };

        inviteRepository.Add(invite);
        await inviteRepository.SaveChangesAsync(ct);

        logger.LogInformation("Invite {InviteId} created for family {FamilyId}", invite.family_invite_id, familyId);

        return FamilyResult.Ok(createdInvite: new CreatedInviteDto(invite.family_invite_id, code, invite.expires_at));
    }
    #endregion

    #region GetActiveInvitesAsync
    public async Task<FamilyResult> GetActiveInvitesAsync(Guid familyId, Guid authUserId, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var now = DateTime.UtcNow;

        // Enkel metadata, nooit de code of hash
        var invites = await inviteRepository.GetActiveByFamilyIdAsync(familyId, now, ct);

        return FamilyResult.Ok(invites: invites.Select(i => i.ToDto()).ToList());
    }
    #endregion

    #region RevokeInviteAsync
    public async Task<FamilyResult> RevokeInviteAsync(Guid familyId, Guid authUserId, int inviteId, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var invite = await inviteRepository.GetByIdAsync(familyId, inviteId, ct);
        // Al ingetrokken telt als niet gevonden
        if (invite is null || invite.revoked_at is not null)
        {
            return FamilyResult.Fail(FamilyError.InviteNotFound);
        }

        invite.revoked_at = timeProvider.GetUtcNow().UtcDateTime;
        await inviteRepository.SaveChangesAsync(ct);

        return FamilyResult.Ok(invites: [invite.ToDto()]);
    }
    #endregion
}
