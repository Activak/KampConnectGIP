using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;

namespace KampConnect.User.Business.Logic.Helpers;

using User = KampConnect.User.Data.Entity.User.User;
using Family = KampConnect.User.Data.Entity.Family.Family;

// Gedeelde autorisatie: user opzoeken en actief lidmaatschap/guardianrol controleren
internal static class FamilyAccess
{
    public sealed record AccessContext(User User, Family Family, UserFamilyMember Membership);

    public static async Task<(AccessContext? Context, FamilyError Error)> CheckAsync(
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        Guid familyId,
        Guid authUserId,
        bool requireGuardian,
        CancellationToken ct)
    {
        var user = await userRepository.GetUserByAuthIdAsync(authUserId, ct);
        if (user is null)
        {
            return (null, FamilyError.UserNotFound);
        }

        var family = await familyRepository.GetFamilyByIdAsync(familyId, ct);
        if (family is null)
        {
            return (null, FamilyError.FamilyNotFound);
        }

        var membership = await familyRepository.GetMembershipAsync(familyId, user.user_id, ct);
        if (membership is null || membership.status != FamilyMemberStatus.active)
        {
            return (null, FamilyError.NotAuthorized);
        }

        if (requireGuardian && membership.role_type != FamilyRoleType.guardian)
        {
            return (null, FamilyError.NotAuthorized);
        }

        return (new AccessContext(user, family, membership), FamilyError.None);
    }
}
