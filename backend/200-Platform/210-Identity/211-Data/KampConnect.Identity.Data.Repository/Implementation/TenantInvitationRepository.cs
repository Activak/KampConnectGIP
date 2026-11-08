using KampConnect.Identity.Data.Context.Implentation;
using KampConnect.Identity.Data.Entity.Tenant.Invitations;
using KampConnect.Identity.Data.Interface.Repository;
using Microsoft.EntityFrameworkCore;


namespace KampConnect.Identity.Data.Repository.Implementation;

public class TenantInvitationRepository(KampConnectIdentityDbContext dbContext) : ITenantInvitationRepository
{
    public void Add(TenantInvitation invitation) => dbContext.TenantInvitations.Add(invitation);

    public Task<TenantInvitation?> GetInvitationByIdAsync(int invitationId, CancellationToken ct = default) =>
        dbContext.TenantInvitations
            .Include(ti => ti.TenantSnapshot)
            .Include(ti => ti.Role)
            .FirstOrDefaultAsync(ti => ti.tenant_invitation_id == invitationId, ct);

    public Task<TenantInvitation?> GetInvitationByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        dbContext.TenantInvitations
            .Include(ti => ti.TenantSnapshot)
            .Include(ti => ti.Role)
            .FirstOrDefaultAsync(ti => ti.token_hash == tokenHash, ct);

    public Task<List<TenantInvitation>> GetInvitationsByTenantIdAsync(int tenantId, CancellationToken ct = default) =>
        dbContext.TenantInvitations
            .AsNoTracking()
            .Include(ti => ti.TenantSnapshot)
            .Include(ti => ti.Role)
            .Where(ti => ti.tenant_id == tenantId)
            .OrderByDescending(ti => ti.creation_timestamp)
            .ToListAsync(ct);

    public Task<TenantInvitation?> GetPendingInvitationAsync(int tenantId, string email, CancellationToken ct = default) =>
        dbContext.TenantInvitations.FirstOrDefaultAsync(ti =>
            ti.tenant_id == tenantId &&
            ti.email == email &&
            ti.status == InvitationStatus.Pending, ct);

}
