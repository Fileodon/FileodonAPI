using Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, AspNetRole, string>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Users og Roles arves fra IdentityDbContext.
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<SFTPConnection> SFTPConnections => Set<SFTPConnection>();
    public DbSet<AspNetRole> AspNetRoles => Set<AspNetRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OrganizationAuditLog> OrganizationAuditLogs => Set<OrganizationAuditLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Beholder AspNetUsers fra den eksisterende initial migration.
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("AspNetUsers");

            entity.Property(user => user.CreatedAt)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasOne(r => r.Organization)
                .WithMany()
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(r => r.PermissionMask)
                .HasColumnType("bigint");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.UserId).IsRequired();
            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");

            entity.HasKey(log => log.Id);

            entity.Property(log => log.Id).HasColumnName("id");
            entity.Property(log => log.Action).HasColumnName("action");
            entity.Property(log => log.RecordId).HasColumnName("record_id");
            entity.Property(log => log.TableName).HasColumnName("table_name");
            entity.Property(log => log.ChangedBy).HasColumnName("changed_by");

            entity.Property(log => log.ChangedAt)
                .HasColumnName("changed_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");

            entity.Property(log => log.NewData)
                .HasColumnName("new_data")
                .HasColumnType("jsonb");

            entity.Property(log => log.OldData)
                .HasColumnName("old_data")
                .HasColumnType("jsonb");

            entity.HasOne(log => log.ChangedByNavigation)
                .WithMany(user => user.AuditLogs)
                .HasForeignKey(log => log.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");

            entity.HasKey(organization => organization.Id);

            entity.Property(organization => organization.Id)
                .HasColumnName("id");

            entity.Property(organization => organization.OrgName)
                .HasColumnName("org_name");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profile");

            entity.HasKey(profile => profile.Id);

            entity.Property(profile => profile.Id).HasColumnName("id");
            entity.Property(profile => profile.DisplayName)
                .HasColumnName("display_name");
        });

        modelBuilder.Entity<UserOrganization>(entity =>
        {
            entity.ToTable("users_organizations");

            entity.HasKey(membership => membership.Id);

            entity.Property(membership => membership.Id)
                .HasColumnName("id");

            entity.Property(membership => membership.OrganizationId)
                .HasColumnName("organization_id");

            entity.Property(membership => membership.UserId)
                .HasColumnName("users_id");

            entity.HasOne(membership => membership.Organization)
                .WithMany(organization => organization.UserOrganizations)
                .HasForeignKey(membership => membership.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(membership => membership.User)
                .WithMany(user => user.UserOrganizations)
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SFTPConnection>(entity =>
        {
            entity.ToTable("sftp_connections");

            entity.HasKey(connection => connection.Id);

            entity.Property(connection => connection.Id)
                .HasColumnName("id");

            entity.Property(connection => connection.DisplayName)
                .HasColumnName("display_name");

            entity.Property(connection => connection.OrganizationId)
                .HasColumnName("organization_id");

            entity.Property(connection => connection.Host)
                .HasColumnName("host");

            entity.Property(connection => connection.Port)
                .HasColumnName("port");

            entity.Property(connection => connection.Status)
                .HasColumnName("status");

            entity.Property(connection => connection.PublicKey)
                .HasColumnName("public_key");

            entity.Property(connection => connection.PrivateKey)
                .HasColumnName("private_key");

            entity.HasOne(connection => connection.Organization)
                .WithMany(organization => organization.SFTPConnections)
                .HasForeignKey(connection => connection.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(permission => permission.Id);
            entity.Property(permission => permission.Id)
                .HasColumnName("id");
            entity.Property(permission => permission.Navn)
                .HasColumnName("navn");
            entity.Property(permission => permission.BitValue)
                .HasColumnName("bit_value");
        });

        modelBuilder.Entity<OrganizationAuditLog>(entity =>
        {
            entity.ToTable("organization_audit_logs");
            entity.Property(e => e.Id)
                .HasColumnName("id");
            entity.Property(e => e.ActorId)
                .HasColumnName("actor_id");
            entity.Property(e => e.Action)
                .HasColumnName("action");
            entity.Property(e => e.Source)
                .HasColumnName("source");
            entity.Property(e => e.OldData)
                .HasColumnName("old_data")
                .HasColumnType("jsonb");
            entity.Property(e => e.NewData)
                .HasColumnName("new_data")
                .HasColumnType("jsonb");
            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp");

            entity.Property(e => e.OrganizationId)
                .HasColumnName("organization_id");

            entity.HasOne(e => e.Organization)
                .WithMany(organization => organization.AuditLogs)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}