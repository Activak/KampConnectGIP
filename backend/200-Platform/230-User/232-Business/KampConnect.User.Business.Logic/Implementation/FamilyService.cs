using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Interfaces;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Business.Logic.Helpers;
using KampConnect.User.Business.Logic.Mapping;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.Extensions.Logging;

namespace KampConnect.User.Business.Logic.Implementation;

using Family = KampConnect.User.Data.Entity.Family.Family;
using User = KampConnect.User.Data.Entity.User.User;

public class FamilyService(
    IFamilyRepository familyRepository,
    IFamilyInviteRepository inviteRepository,
    IUserRepository userRepository,
    TimeProvider timeProvider,
    ILogger<FamilyService> logger) : IFamilyService
{
    #region GetFamiliesAsync
    public async Task<FamilyResult> GetFamiliesAsync(Guid authUserId, CancellationToken ct = default)
    {
        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return FamilyResult.Fail(FamilyError.UserNotFound);
        }

        // Enkel actieve lidmaatschappen, pending/removed families blijven verborgen
        var memberships = await familyRepository.GetActiveMembershipsByUserIdAsync(user.user_id, ct);

        var families = memberships
            .Where(m => m.Family is not null)
            .Select(m => m.Family!.ToDto(m.role_type))
            .ToList();

        return FamilyResult.Ok(families: families);
    }
    #endregion

    #region GetFamilyByIdAsync
    public async Task<FamilyResult> GetFamilyByIdAsync(Guid familyId, Guid authUserId, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: false, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        return FamilyResult.Ok(family: access.Family.ToDto(access.Membership.role_type));
    }
    #endregion

    #region GetFamilyMembersAsync
    public async Task<FamilyResult> GetFamilyMembersAsync(Guid familyId, Guid authUserId, FamilyMemberStatus? familyMemberStatus, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: false, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        // Enkel guardians zien aanvragen en verwijderde/geweigerde leden
        var status = access.Membership.role_type == FamilyRoleType.guardian
            ? familyMemberStatus
            : FamilyMemberStatus.active;

        var members = await familyRepository.GetMembersByFamilyIdAsync(familyId, status, ct);

        return FamilyResult.Ok(members: members.Select(m => m.ToDto()).ToList());
    }
    #endregion

    #region AddFamilyAsync
    public async Task<FamilyResult> AddFamilyAsync(Guid authUserId, FamilyCreate familyCreate, CancellationToken ct = default)
    {
        var validation = ValidateName(familyCreate.Name);
        if (validation is not null)
        {
            return FamilyResult.Invalid(validation);
        }

        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return FamilyResult.Fail(FamilyError.UserNotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var family = new Family
        {
            family_id = Guid.NewGuid(),
            name = familyCreate.Name.Trim(),
            invite_enabled = true,
            created_at = now,
            updated_at = now
        };

        // Maker wordt meteen actieve guardian en primair contact
        var member = new UserFamilyMember
        {
            family_id = family.family_id,
            user_id = user.user_id,
            role_type = FamilyRoleType.guardian,
            status = FamilyMemberStatus.active,
            is_primary_contact = true,
            approved_at = now,
            created_at = now,
            updated_at = now,
            Family = family
        };

        familyRepository.AddFamily(family);
        familyRepository.AddMember(member);
        await familyRepository.SaveChangesAsync(ct);

        logger.LogInformation("Family {FamilyId} created by user {UserId}", family.family_id, user.user_id);

        return FamilyResult.Ok(family: family.ToDto(FamilyRoleType.guardian));
    }
    #endregion

    #region UpdateFamilyAsync
    public async Task<FamilyResult> UpdateFamilyAsync(Guid familyId, Guid authUserId, FamilyUpdate familyUpdate, CancellationToken ct = default)
    {
        if (familyUpdate.Name is not null)
        {
            var validation = ValidateName(familyUpdate.Name);
            if (validation is not null)
            {
                return FamilyResult.Invalid(validation);
            }
        }

        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var family = access.Family;

        if (familyUpdate.Name is not null)
        {
            family.name = familyUpdate.Name.Trim();
        }

        if (familyUpdate.InviteEnabled is not null)
        {
            family.invite_enabled = familyUpdate.InviteEnabled.Value;
        }

        family.updated_at = timeProvider.GetUtcNow().UtcDateTime;
        await familyRepository.SaveChangesAsync(ct);

        return FamilyResult.Ok(family: family.ToDto(access.Membership.role_type));
    }
    #endregion

    #region UpdateMemberAsync
    public async Task<FamilyResult> UpdateMemberAsync(Guid familyId, Guid authUserId, int memberId, MemberUpdate memberUpdate, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var member = await familyRepository.GetMemberByIdAsync(familyId, memberId, ct);
        if (member is null || member.status != FamilyMemberStatus.active)
        {
            return FamilyResult.Fail(FamilyError.MemberNotFound);
        }

        // Laatste guardian mag niet gedegradeerd worden
        if (memberUpdate.Role is not null
            && member.role_type == FamilyRoleType.guardian
            && memberUpdate.Role != FamilyRoleType.guardian
            && await familyRepository.CountActiveGuardiansAsync(familyId, ct) <= 1)
        {
            return FamilyResult.Fail(FamilyError.LastGuardian);
        }

        if (memberUpdate.Role is not null)
        {
            member.role_type = memberUpdate.Role.Value;
        }

        if (memberUpdate.IsPrimaryContact is not null)
        {
            member.is_primary_contact = memberUpdate.IsPrimaryContact.Value;
        }

        member.updated_at = timeProvider.GetUtcNow().UtcDateTime;
        await familyRepository.SaveChangesAsync(ct);

        return FamilyResult.Ok(member: member.ToDto());
    }
    #endregion

    #region AddChildAsync
    public async Task<FamilyResult> AddChildAsync(Guid authUserId, Guid familyId, ChildCreate childCreate, CancellationToken ct = default)
    {
        var validation = ValidateChild(childCreate, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        if (validation is not null)
        {
            return FamilyResult.Invalid(validation);
        }

        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Kind heeft geen eigen login, dus geen auth_user_id
        var child = new User
        {
            user_id = Guid.NewGuid(),
            auth_user_id = null,
            first_name = childCreate.FirstName.Trim(),
            last_name = childCreate.LastName.Trim(),
            birth_date = childCreate.BirthDate,
            preferred_language = childCreate.PreferredLanguage,
            created_at = now,
            updated_at = now
        };

        var member = new UserFamilyMember
        {
            family_id = familyId,
            user_id = child.user_id,
            role_type = FamilyRoleType.child,
            status = FamilyMemberStatus.active,
            is_primary_contact = false,
            approved_by_user_id = access.User.user_id,
            approved_at = now,
            created_at = now,
            updated_at = now,
            User = child
        };

        userRepository.Add(child);
        familyRepository.AddMember(member);
        await familyRepository.SaveChangesAsync(ct);

        logger.LogInformation("Child {ChildId} added to family {FamilyId}", child.user_id, familyId);

        return FamilyResult.Ok(member: member.ToDto());
    }
    #endregion

    #region JoinFamilyAsync
    public async Task<FamilyResult> JoinFamilyAsync(JoinFamily joinFamily, Guid authUserId, CancellationToken ct = default)
    {
        // Ongeldige lengte meteen weigeren, zonder databasequery
        var code = InviteCodeHelper.Normalize(joinFamily.Code ?? string.Empty);
        if (code.Length != InviteCodeHelper.CodeLength)
        {
            return FamilyResult.Fail(FamilyError.InvalidOrExpiredCode);
        }

        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return FamilyResult.Fail(FamilyError.UserNotFound);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Opzoeken via hash; onbekend, ingetrokken en verlopen geven bewust dezelfde fout
        var invite = await inviteRepository.GetByCodeHashAsync(InviteCodeHelper.Hash(code), ct);
        if (invite is null || invite.revoked_at is not null || invite.expires_at <= now)
        {
            return FamilyResult.Fail(FamilyError.InvalidOrExpiredCode);
        }

        if (invite.Family is null || !invite.Family.invite_enabled)
        {
            return FamilyResult.Fail(FamilyError.InvitesDisabled);
        }

        var existing = await familyRepository.GetMembershipAsync(invite.family_id, user.user_id, ct);
        if (existing is not null
            && existing.status is FamilyMemberStatus.active or FamilyMemberStatus.pending)
        {
            return FamilyResult.Fail(FamilyError.AlreadyMember);
        }

        UserFamilyMember member;
        if (existing is not null)
        {
            // Eerder geweigerd of verwijderd: opnieuw aanvragen
            member = existing;
            member.status = FamilyMemberStatus.pending;
            member.role_type = FamilyRoleType.guardian;
            member.invite_id = invite.family_invite_id;
            member.approved_by_user_id = null;
            member.approved_at = null;
            member.requested_at = now;
            member.updated_at = now;
        }
        else
        {
            member = new UserFamilyMember
            {
                family_id = invite.family_id,
                user_id = user.user_id,
                role_type = FamilyRoleType.guardian,
                status = FamilyMemberStatus.pending,
                is_primary_contact = false,
                invite_id = invite.family_invite_id,
                requested_at = now,
                created_at = now,
                updated_at = now,
                User = user
            };
            familyRepository.AddMember(member);
        }

        // Gebruik tellen; zelfde DbContext, dus één SaveChanges volstaat
        invite.used_count = (invite.used_count ?? 0) + 1;
        await familyRepository.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} requested to join family {FamilyId}", user.user_id, invite.family_id);

        return FamilyResult.Ok(member: member.ToDto());
    }
    #endregion

    #region RemoveMemberAsync
    public async Task<FamilyResult> RemoveMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: false, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        // Guardians mogen iedereen verwijderen, anderen enkel zichzelf
        var isSelf = access.Membership.family_member_id == memberId;
        if (!isSelf && access.Membership.role_type != FamilyRoleType.guardian)
        {
            return FamilyResult.Fail(FamilyError.NotAuthorized);
        }

        var member = await familyRepository.GetMemberByIdAsync(familyId, memberId, ct);
        if (member is null || member.status == FamilyMemberStatus.removed)
        {
            return FamilyResult.Fail(FamilyError.MemberNotFound);
        }

        // Familie mag nooit zonder actieve guardian vallen
        if (member.role_type == FamilyRoleType.guardian
            && member.status == FamilyMemberStatus.active
            && await familyRepository.CountActiveGuardiansAsync(familyId, ct) <= 1)
        {
            return FamilyResult.Fail(FamilyError.LastGuardian);
        }

        // Soft delete: historiek blijft bewaard
        member.status = FamilyMemberStatus.removed;
        member.is_primary_contact = false;
        member.updated_at = timeProvider.GetUtcNow().UtcDateTime;
        await familyRepository.SaveChangesAsync(ct);

        return FamilyResult.Ok(member: member.ToDto());
    }
    #endregion

    #region ApproveMemberAsync
    public Task<FamilyResult> ApproveMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default) =>
        DecidePendingAsync(familyId, authUserId, memberId, FamilyMemberStatus.active, ct);
    #endregion

    #region RejectMemberAsync
    public Task<FamilyResult> RejectMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default) =>
        DecidePendingAsync(familyId, authUserId, memberId, FamilyMemberStatus.rejected, ct);
    #endregion

    #region Helpers
    // Gedeelde logica voor goedkeuren en weigeren van een aanvraag
    private async Task<FamilyResult> DecidePendingAsync(Guid familyId, Guid authUserId, int memberId, FamilyMemberStatus newStatus, CancellationToken ct = default)
    {
        var (access, error) = await FamilyAccess.CheckAsync(userRepository, familyRepository, familyId, authUserId, requireGuardian: true, ct);
        if (access is null)
        {
            return FamilyResult.Fail(error);
        }

        var member = await familyRepository.GetMemberByIdAsync(familyId, memberId, ct);
        if (member is null)
        {
            return FamilyResult.Fail(FamilyError.MemberNotFound);
        }

        if (member.status != FamilyMemberStatus.pending)
        {
            return FamilyResult.Fail(FamilyError.NotPending);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        member.status = newStatus;
        member.approved_by_user_id = access.User.user_id;
        // approved_by bewaart ook wie weigerde, approved_at enkel bij goedkeuring
        member.approved_at = newStatus == FamilyMemberStatus.active ? now : null;
        member.updated_at = now;
        await familyRepository.SaveChangesAsync(ct);

        logger.LogInformation("Member {MemberId} in family {FamilyId} set to {Status}", memberId, familyId, newStatus);

        return FamilyResult.Ok(member: member.ToDto());
    }

    private static Dictionary<string, string[]>? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new() { ["Name"] = ["Naam is verplicht."] };
        }

        if (name.Trim().Length > 255)
        {
            return new() { ["Name"] = ["Naam mag maximaal 255 tekens zijn."] };
        }

        return null;
    }

    private static Dictionary<string, string[]>? ValidateChild(ChildCreate child, DateOnly today)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(child.FirstName))
        {
            errors["FirstName"] = ["Voornaam is verplicht."];
        }

        if (string.IsNullOrWhiteSpace(child.LastName))
        {
            errors["LastName"] = ["Achternaam is verplicht."];
        }

        if (child.BirthDate > today)
        {
            errors["BirthDate"] = ["Geboortedatum mag niet in de toekomst liggen."];
        }

        if (string.IsNullOrWhiteSpace(child.PreferredLanguage))
        {
            errors["PreferredLanguage"] = ["Taal is verplicht."];
        }

        return errors.Count > 0 ? errors : null;
    }
    #endregion
}
