using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Test;

public sealed class AuditLogRepositoryTests
{
    private const int TargetOrganizationId = 1001;
    private const int OtherOrganizationId = 2002;
    private const string ActorId = "audit-log-test-actor";

    [Fact]
    public async Task GetOrganizationAuditLogsFromOrganizationId_returns_only_logs_for_organization()
    {
        await using ApplicationDbContext context = CreateContext();
        SeedAuditLogs(context);
        AuditLogRepository repository = new(context);

        IEnumerable<OrganizationAuditLog> logs =
            await repository.GetOrganizationAuditLogsFromOrganizationId(TargetOrganizationId);

        OrganizationAuditLog log = Assert.Single(logs);
        Assert.Equal(TargetOrganizationId, log.OrganizationId);
    }

    [Fact]
    public async Task GetSFTPConnectionAuditLogsFromOrganizationId_filters_and_loads_connection()
    {
        await using ApplicationDbContext context = CreateContext();
        SeedAuditLogs(context);
        AuditLogRepository repository = new(context);

        IEnumerable<SFTPConnectionAuditLog> logs =
            await repository.GetSFTPConnectionAuditLogsFromOrganizationId(TargetOrganizationId);

        SFTPConnectionAuditLog log = Assert.Single(logs);
        Assert.NotNull(log.SFTPConnnection);
        Assert.Equal(TargetOrganizationId, log.SFTPConnnection.OrganizationId);
    }

    [Fact]
    public async Task GetRoleAuditLogsFromOrganizationId_filters_and_loads_role()
    {
        await using ApplicationDbContext context = CreateContext();
        SeedAuditLogs(context);
        AuditLogRepository repository = new(context);

        IEnumerable<RoleAuditLog> logs =
            await repository.GetRoleAuditLogsFromOrganizationId(TargetOrganizationId);

        RoleAuditLog log = Assert.Single(logs);
        Assert.NotNull(log.Role);
        Assert.Equal(TargetOrganizationId, log.Role.OrganizationId);
    }

    [Fact]
    public async Task GetUserOrganizationAuditLogsFromOrganizationId_filters_and_loads_membership()
    {
        await using ApplicationDbContext context = CreateContext();
        SeedAuditLogs(context);
        AuditLogRepository repository = new(context);

        IEnumerable<UserOrganizationAuditLog> logs =
            await repository.GetUserOrganizationAuditLogsFromOrganizationId(TargetOrganizationId);

        UserOrganizationAuditLog log = Assert.Single(logs);
        Assert.NotNull(log.UserOrganization);
        Assert.Equal(TargetOrganizationId, log.UserOrganization.OrganizationId);
    }

    private static ApplicationDbContext CreateContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        ApplicationDbContext context = new(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    private static void SeedAuditLogs(ApplicationDbContext context)
    {
        User actor = new()
        {
            Id = ActorId,
            UserName = "audit-log-test-actor"
        };
        Organization targetOrganization = CreateOrganization(TargetOrganizationId, actor);
        Organization otherOrganization = CreateOrganization(OtherOrganizationId, actor);
        SFTPConnection targetConnection = CreateConnection(targetOrganization);
        SFTPConnection otherConnection = CreateConnection(otherOrganization);
        AspNetRole targetRole = CreateRole("target-role", targetOrganization);
        AspNetRole otherRole = CreateRole("other-role", otherOrganization);
        UserOrganization targetMembership = CreateMembership(actor, targetRole, targetOrganization);
        UserOrganization otherMembership = CreateMembership(actor, otherRole, otherOrganization);

        context.Users.Add(actor);
        context.OrganizationAuditLogs.AddRange(
            CreateOrganizationAuditLog(targetOrganization),
            CreateOrganizationAuditLog(otherOrganization));
        context.SFTPConnectionAuditLogs.AddRange(
            CreateSFTPConnectionAuditLog(targetConnection),
            CreateSFTPConnectionAuditLog(otherConnection));
        context.RoleAuditLogs.AddRange(
            CreateRoleAuditLog(targetRole),
            CreateRoleAuditLog(otherRole));
        context.UserOrganizationAuditLogs.AddRange(
            CreateUserOrganizationAuditLog(targetMembership),
            CreateUserOrganizationAuditLog(otherMembership));
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

    private static Organization CreateOrganization(int id, User owner) =>
        new()
        {
            Id = id,
            UserOwnerId = owner.Id,
            OrgName = $"Organization {id}",
            WorkspaceURL = $"organization-{id}.example.com",
            ContactEmail = $"owner-{id}@example.com"
        };

    private static SFTPConnection CreateConnection(Organization organization) =>
        new()
        {
            DisplayName = $"Connection for {organization.Id}",
            Organization = organization,
            Host = $"sftp-{organization.Id}.example.com"
        };

    private static AspNetRole CreateRole(string id, Organization organization) =>
        new()
        {
            Id = id,
            Name = id,
            Organization = organization
        };

    private static UserOrganization CreateMembership(
        User user,
        AspNetRole role,
        Organization organization) =>
        new()
        {
            User = user,
            Role = role,
            Organization = organization
        };

    private static OrganizationAuditLog CreateOrganizationAuditLog(Organization organization) =>
        new()
        {
            ActorId = ActorId,
            Source = "test",
            Organization = organization
        };

    private static SFTPConnectionAuditLog CreateSFTPConnectionAuditLog(SFTPConnection connection) =>
        new()
        {
            ActorId = ActorId,
            Source = "test",
            SFTPConnnection = connection
        };

    private static RoleAuditLog CreateRoleAuditLog(AspNetRole role) =>
        new()
        {
            ActorId = ActorId,
            Source = "test",
            Role = role
        };

    private static UserOrganizationAuditLog CreateUserOrganizationAuditLog(
        UserOrganization membership) =>
        new()
        {
            ActorId = ActorId,
            Source = "test",
            UserOrganization = membership
        };
}
