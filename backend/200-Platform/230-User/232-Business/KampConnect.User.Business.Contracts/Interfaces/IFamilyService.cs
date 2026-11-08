using KampConnect.User.Business.Contracts.ClassTypes.Results;
using KampConnect.User.Business.Contracts.Requests;
using KampConnect.User.Data.Entity.Family;

namespace KampConnect.User.Business.Contracts.Interfaces;

public interface IFamilyService
{
    public Task<FamilyResult> GetFamiliesAsync(Guid authUserId, CancellationToken ct = default);

    public Task<FamilyResult> GetFamilyByIdAsync(Guid familyId, Guid authUserId, CancellationToken ct = default);

    public Task<FamilyResult> GetFamilyMembersAsync(Guid familyId, Guid authUserId, FamilyMemberStatus? familyMemberStatus, CancellationToken ct = default);

    public Task<FamilyResult> AddFamilyAsync(Guid authUserId, FamilyCreate familyCreate, CancellationToken ct = default);

    public Task<FamilyResult> UpdateFamilyAsync(Guid familyId, Guid authUserId, FamilyUpdate familyUpdate, CancellationToken ct = default);

    public Task<FamilyResult> UpdateMemberAsync(Guid familyId, Guid authUserId, int memberId, MemberUpdate memberUpdate, CancellationToken ct = default);

    public Task<FamilyResult> AddChildAsync(Guid authUserId, Guid familyId, ChildCreate childCreate, CancellationToken ct = default);

    public Task<FamilyResult> JoinFamilyAsync(JoinFamily joinFamily, Guid authUserId, CancellationToken ct = default);

    public Task<FamilyResult> RemoveMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default);

    public Task<FamilyResult> ApproveMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default);

    public Task<FamilyResult> RejectMemberAsync(Guid familyId, Guid authUserId, int memberId, CancellationToken ct = default);
}
