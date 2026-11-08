using KampConnect.User.Data.Context.Implentation;
using KampConnect.User.Data.Entity.Family;
using KampConnect.User.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Repository.Implentation;

public class FamilyInviteRepository(KampConnectUserDbContext dbContext) : IFamilyInviteRepository
{
    public Task<UserFamilyInvite?> GetByCodeHashAsync(byte[] codeHash, CancellationToken ct = default) =>
        dbContext.UserFamilyInvites
            .Include(i => i.Family)
            .FirstOrDefaultAsync(i => i.code_hash == codeHash, ct);

    public Task<UserFamilyInvite?> GetByIdAsync(Guid familyId, int familyInviteId, CancellationToken ct = default) =>
        dbContext.UserFamilyInvites.FirstOrDefaultAsync(i => i.family_id == familyId && i.family_invite_id == familyInviteId, ct);

    public Task<List<UserFamilyInvite>> GetActiveByFamilyIdAsync(Guid familyId, DateTime now, CancellationToken ct = default) =>
        dbContext.UserFamilyInvites
            .AsNoTracking()
            .Where(i => i.family_id == familyId && i.revoked_at == null && i.expires_at > now)
            .OrderBy(i => i.expires_at)
            .ToListAsync(ct);

    public void Add(UserFamilyInvite invite) => dbContext.UserFamilyInvites.Add(invite);

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
