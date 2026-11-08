using KampConnect.User.Business.Contracts.ClassTypes.Results;

namespace KampConnect.User.Business.Contracts.Interfaces;

public interface IFamilyInviteService
{
    public Task<FamilyResult> CreateInviteAsync(Guid familyId, Guid authUserId, CancellationToken ct = default);

    public Task<FamilyResult> GetActiveInvitesAsync(Guid familyId, Guid authUserId, CancellationToken ct = default);

    public Task<FamilyResult> RevokeInviteAsync(Guid familyId, Guid authUserId, int inviteId, CancellationToken ct = default);
}
