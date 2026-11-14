using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Camp.Data.Interface.Repository;

using Camp = KampConnect.Camp.Data.Entity.Camps.Camp;

public interface ICampRepository
{
    public Task<Camp?> GetCampByIdAsync(Guid campId, CancellationToken ct = default);

    public Task<List<Camp>> GetCampsByTenantIdAsync(Guid tenantId, CancellationToken ct = default);

    public void Add(Camp camp);

    public Task SaveChangesAsync(CancellationToken ct = default);
}

