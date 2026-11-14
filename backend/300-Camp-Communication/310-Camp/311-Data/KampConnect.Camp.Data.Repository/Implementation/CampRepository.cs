using KampConnect.Camp.Data.Context.Implementation;
using KampConnect.Camp.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.Camp.Data.Repository.Implementation;

public class CampRepository(KampConnectCampDbContext dbContext) : ICampRepository
{
    public void Add(Entity.Camps.Camp camp) => dbContext.Camps.Add(camp);

    public Task<Entity.Camps.Camp?> GetCampByIdAsync(Guid campId, CancellationToken ct = default) =>
         dbContext.Camps.FirstOrDefaultAsync(c => c.camp_id == campId, ct);

    public Task<List<Entity.Camps.Camp>> GetCampsByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        var query = dbContext.Camps
            .AsNoTracking()
            .Where(c => c.tenant_id == tenantId);
       
        return query.ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}

