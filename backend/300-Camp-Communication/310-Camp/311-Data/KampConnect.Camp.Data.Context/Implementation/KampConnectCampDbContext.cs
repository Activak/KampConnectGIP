using Microsoft.EntityFrameworkCore;

namespace KampConnect.Camp.Data.Context.Implementation;

public class KampConnectCampDbContext(
    DbContextOptions<KampConnectCampDbContext> options) : DbContext(options)
{
    public DbSet<Entity.Camps.Camp> Camps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Entity.Camps.Camp>(entity =>
        {
            entity.HasKey(e => e.camp_id);
            entity.Property(e => e.tenant_id).IsRequired();
            entity.Property(e => e.name).IsRequired();
            entity.Property(e => e.start_date).IsRequired();
            entity.Property(e => e.end_date).IsRequired();
            entity.Property(e => e.status).IsRequired();
            entity.Property(e => e.is_published).IsRequired();
            entity.Property(e => e.published_at).IsRequired();
            entity.Property(e => e.created_at).IsRequired();
        });
    }
}
