using Domain.Service;
using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Test;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task Commit_persists_changes_from_multiple_repositories()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.OrganizationRepository.PostAsync(new Organization { OrgName = "Acme" });
        await unitOfWork.ProfileRepository.PostAsync(new Profile { DisplayName = "Ada" });
        await unitOfWork.CommitAsync();

        Assert.Equal(1, await context.Organizations.CountAsync());
        Assert.Equal(1, await context.Profiles.CountAsync());
    }

    [Fact]
    public async Task Rollback_discards_changes_from_multiple_repositories()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.OrganizationRepository.PostAsync(new Organization { OrgName = "Acme" });
        await unitOfWork.ProfileRepository.PostAsync(new Profile { DisplayName = "Ada" });
        await unitOfWork.RollbackAsync();

        Assert.Empty(await context.Organizations.ToListAsync());
        Assert.Empty(await context.Profiles.ToListAsync());
    }

    [Fact]
    public async Task BeginTransaction_rejects_an_active_transaction()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.BeginTransactionAsync());

        Assert.Contains("already active", exception.Message);
        await unitOfWork.RollbackAsync();
    }

    [Fact]
    public async Task Commit_and_rollback_require_an_active_transaction()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        InvalidOperationException commitException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.CommitAsync());
        InvalidOperationException rollbackException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => unitOfWork.RollbackAsync());

        Assert.Contains("No unit-of-work transaction is active", commitException.Message);
        Assert.Contains("No unit-of-work transaction is active", rollbackException.Message);
    }

    [Fact]
    public async Task Automatic_transactions_can_be_disabled_and_restored_by_scope()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        Assert.True(unitOfWork.AutomaticTransactionsEnabled);

        using (unitOfWork.DisableAutomaticTransactions())
        {
            Assert.False(unitOfWork.AutomaticTransactionsEnabled);

            using (unitOfWork.DisableAutomaticTransactions())
            {
                Assert.False(unitOfWork.AutomaticTransactionsEnabled);
            }

            Assert.False(unitOfWork.AutomaticTransactionsEnabled);
        }

        Assert.True(unitOfWork.AutomaticTransactionsEnabled);
    }

    [Fact]
    public async Task Repository_operation_joins_explicit_unit_of_work_transaction()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.OrganizationRepository.PostAsync(new Organization { OrgName = "Acme" });

        Assert.NotNull(context.Database.CurrentTransaction);

        await unitOfWork.RollbackAsync();

        Assert.Empty(await context.Organizations.ToListAsync());
    }

    [Fact]
    public async Task Automatic_transaction_rolls_back_when_repository_save_fails()
    {
        await using ApplicationDbContext context = CreateContext();
        using UnitOfWork unitOfWork = CreateUnitOfWork(context);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => unitOfWork.UserOrganizationRepository.PostAsync(
                new UserOrganization
                {
                    UserId = "missing-user",
                    OrganizationId = 999
                }));

        Assert.Empty(await context.UserOrganizations.ToListAsync());
        Assert.Null(context.Database.CurrentTransaction);
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

    private static UnitOfWork CreateUnitOfWork(ApplicationDbContext context)
    {
        RepositoryTransactionSettings transactionSettings = new();

        return new UnitOfWork(
            context,
            transactionSettings,
            new AuditLogRepository(context, transactionSettings),
            new OrganizationRepository(context, transactionSettings),
            new ProfileRepository(context, transactionSettings),
            new RoleRepository(context, transactionSettings),
            new RolesOrganizationRepository(context, transactionSettings),
            new SFTPConnectionRepository(context, transactionSettings),
            new SFTPConnectionAuthCodeRepository(context, transactionSettings),
        new UserOrganizationRepository(context, transactionSettings),
            new UserRepository(context, transactionSettings));
    }
}
