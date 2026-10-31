using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Context.Implentation;

public class KampConnectUserDbContext(
    DbContextOptions<KampConnectUserDbContext> options) : DbContext(options)
{
    public DbSet<Entity.User.User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure the User entity
        modelBuilder.Entity<Entity.User.User>(entity =>
        {
            entity.HasKey(e => e.user_id);
            entity.HasIndex(e => e.auth_user_id).IsUnique();
            entity.Property(e => e.first_name).IsRequired();
            entity.Property(e => e.last_name).IsRequired();
            entity.Property(e => e.birth_date).IsRequired();
            entity.Property(e => e.preferred_language).HasMaxLength(255);
            entity.Property(e => e.avatar_file_id).IsRequired();
            entity.Property(e => e.status).IsRequired();
            entity.Property(e => e.created_at).IsRequired();
        });
    }
}

