using KampConnect.User.Data.Entity.Family;
using Microsoft.EntityFrameworkCore;

namespace KampConnect.User.Data.Context.Implentation;

public class KampConnectUserDbContext(
    DbContextOptions<KampConnectUserDbContext> options) : DbContext(options)
{
    public DbSet<Entity.User.User> Users { get; set; }

    public DbSet<Entity.Address.UserAddress> UserAddress { get; set; }

    public DbSet<Entity.Family.Family> Families { get; set; }

    public DbSet<Entity.Family.UserFamilyMember> UserFamilyMembers { get; set; }

    public DbSet<Entity.Family.UserFamilyInvite> UserFamilyInvites { get; set; }

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
            entity.Property(e => e.status).IsRequired();
            entity.Property(e => e.created_at).IsRequired();
        });
        // Configure the UserAddress entity
        modelBuilder.Entity<Entity.Address.UserAddress>(entity =>
        {
            entity.HasKey(e => e.user_address_id);
            entity.Property(e => e.user_id).IsRequired();
            entity.Property(e => e.address_type).IsRequired();
            entity.Property(e => e.street).HasMaxLength(255);
            entity.Property(e => e.house_number).HasMaxLength(50);
            entity.Property(e => e.box).HasMaxLength(50);
            entity.Property(e => e.postal_code).HasMaxLength(20);
            entity.Property(e => e.city).HasMaxLength(100);
            entity.Property(e => e.country_code).HasMaxLength(10);
            entity.Property(e => e.is_primary).IsRequired();
            entity.Property(e => e.created_at).IsRequired();
        });

        modelBuilder.Entity<Entity.Family.Family>(entity =>
        {
            entity.HasKey(e => e.family_id);
            entity.Property(e => e.name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.created_at).IsRequired();
        });

        modelBuilder.Entity<Entity.Family.UserFamilyMember>(entity =>
        {
            entity.HasKey(e => e.family_member_id);
            entity.Property(e => e.family_id).IsRequired();
            entity.Property(e => e.user_id).IsRequired();
            entity.Property(e => e.role_type).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(e => e.is_primary_contact).IsRequired();
            entity.Property(e => e.created_at).IsRequired();

            // Een gebruiker kan maar één keer lid zijn van dezelfde familie
            entity.HasIndex(e => new { e.family_id, e.user_id }).IsUnique();

            // Familie weg = lidmaatschappen weg
            entity.HasOne(e => e.Family)
                  .WithMany(f => f.Members)
                  .HasForeignKey(e => e.family_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Gebruiker weg = zijn lidmaatschappen weg
            entity.HasOne(e => e.User)
                  .WithMany(u => u.FamilyMemberships)
                  .HasForeignKey(e => e.user_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Goedkeurder weg = lidmaatschap blijft, enkel de verwijzing vervalt
            entity.HasOne(e => e.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(e => e.approved_by_user_id)
                  .OnDelete(DeleteBehavior.SetNull);

            // Invite weg = lidmaatschap blijft
            entity.HasOne(e => e.Invite)
                  .WithMany(i => i.Members)
                  .HasForeignKey(e => e.invite_id)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Entity.Family.UserFamilyInvite>(entity =>
        {
            entity.HasKey(e => e.family_invite_id);
            entity.Property(e => e.family_id).IsRequired();
            entity.HasIndex(e => e.code_hash).IsUnique();
            entity.Property(e => e.code_hash).IsRequired();
            entity.Property(e => e.created_by_user_id).IsRequired();
            entity.Property(e => e.expires_at).IsRequired();
            entity.Property(e => e.created_at).IsRequired();

            // Snel actieve invites per familie opzoeken
            entity.HasIndex(e => new { e.family_id, e.expires_at });

            // Familie weg = invites weg
            entity.HasOne(e => e.Family)
                  .WithMany(f => f.Invites)
                  .HasForeignKey(e => e.family_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Maker kan niet verwijderd worden zolang er invites van hem bestaan
            entity.HasOne(e => e.CreatedBy)
                  .WithMany()
                  .HasForeignKey(e => e.created_by_user_id)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

