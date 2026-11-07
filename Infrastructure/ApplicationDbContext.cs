using Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User, AspNetRole, string>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Users og Roles arves fra IdentityDbContext.
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<SFTPConnection> SFTPConnections => Set<SFTPConnection>();
    public DbSet<AspNetRole> AspNetRoles => Set<AspNetRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OrganizationAuditLog> OrganizationAuditLogs => Set<OrganizationAuditLog>();
    public DbSet<SFTPConnectionAuditLog> SFTPConnectionAuditLogs => Set<SFTPConnectionAuditLog>();
    public DbSet<UserOrganizationAuditLog> UserOrganizationAuditLogs => Set<UserOrganizationAuditLog>();
    public DbSet<RoleAuditLog> RoleAuditLogs => Set<RoleAuditLog>();

    // Kan bruges til at se kørte sql commands - brug kun i under development!
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information).EnableSensitiveDataLogging();
    //     base.OnConfiguring(optionsBuilder);
    // }

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

            ModifyBaseAuditLogColumns(entity);

            entity.Property(e => e.OrganizationId)
                .HasColumnName("organization_id");

            entity.HasOne(e => e.Organization)
                .WithMany(organization => organization.AuditLogs)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoleAuditLog>(entity =>
        {
            entity.ToTable("role_audit_logs");

            ModifyBaseAuditLogColumns(entity);

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.HasOne(e => e.Role)
                .WithMany(role => role.AuditLogs)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SFTPConnectionAuditLog>(entity =>
        {
            entity.ToTable("sftp_connection_audit_logs");

            ModifyBaseAuditLogColumns(entity);

            entity.Property(e => e.SFTPConnectionID)
                .HasColumnName("sftp_connection_id");

            entity.HasOne(e => e.SFTPConnnection)
                .WithMany(sftpConnection => sftpConnection.AuditLogs)
                .HasForeignKey(e => e.SFTPConnectionID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserOrganizationAuditLog>(entity =>
        {
            entity.ToTable("user_organization_audit_logs");

            ModifyBaseAuditLogColumns(entity);

            entity.Property(e => e.UserOrganizationId)
                .HasColumnName("user_organization_id");

            entity.HasOne(e => e.UserOrganization)
                .WithMany(userOrganization => userOrganization.AuditLogs)
                .HasForeignKey(e => e.UserOrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });


    }

    private void ModifyBaseAuditLogColumns<TEntity>(EntityTypeBuilder<TEntity> entity)
        where TEntity : BaseAuditLog
    {
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
    }
}