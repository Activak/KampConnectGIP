using KampConnect.User.Data.Context.Implentation;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Repository.Implentation;

public class FamilyRepository(KampConnectUserDbContext dbContext) : IFamilyRepository
{
    public Task<List<UserFamilyMember>> GetActiveMembershipsByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        dbContext.UserFamilyMembers
            .AsNoTracking()
            .Include(m => m.Family)
            .Where(m => m.user_id == userId && m.status == FamilyMemberStatus.active)
            .ToListAsync(ct);

    public Task<Family?> GetFamilyByIdAsync(Guid familyId, CancellationToken ct = default) =>
        dbContext.Families.FirstOrDefaultAsync(f => f.family_id == familyId, ct);

    public Task<List<UserFamilyMember>> GetMembersByFamilyIdAsync(Guid familyId, FamilyMemberStatus? status = null, CancellationToken ct = default)
    {
        var query = dbContext.UserFamilyMembers
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.family_id == familyId);

        if (status is not null)
        {
            query = query.Where(m => m.status == status);
        }

        return query.ToListAsync(ct);
    }

    public Task<UserFamilyMember?> GetMembershipAsync(Guid familyId, Guid userId, CancellationToken ct = default) =>
        dbContext.UserFamilyMembers.FirstOrDefaultAsync(m => m.family_id == familyId && m.user_id == userId, ct);

    public Task<UserFamilyMember?> GetMemberByIdAsync(Guid familyId, int familyMemberId, CancellationToken ct = default) =>
        dbContext.UserFamilyMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.family_id == familyId && m.family_member_id == familyMemberId, ct);

    public Task<int> CountActiveGuardiansAsync(Guid familyId, CancellationToken ct = default) =>
        dbContext.UserFamilyMembers.CountAsync(m =>
            m.family_id == familyId &&
            m.role_type == FamilyRoleType.guardian &&
            m.status == FamilyMemberStatus.active, ct);

    public void AddFamily(Family family) => dbContext.Families.Add(family);

    public void AddMember(UserFamilyMember member) => dbContext.UserFamilyMembers.Add(member);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
