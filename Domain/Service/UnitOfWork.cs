using Domain.Interface;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Service;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    IRepositoryTransactionSettings transactionSettings,
    IOrganizationRepository organizationRepository,
    IProfileRepository profileRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRoleRepository roleRepository,
    ISFTPConnectionRepository sftpConnectionRepository,
    IUserOrganizationRepository userOrganizationRepository,
    IUserRepository userRepository) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IOrganizationRepository OrganizationRepository => organizationRepository;
    public IProfileRepository ProfileRepository => profileRepository;
    public IRefreshTokenRepository RefreshTokenRepository => refreshTokenRepository;
    public IRoleRepository RoleRepository => roleRepository;
    public ISFTPConnectionRepository SFTPConnectionRepository => sftpConnectionRepository;
    public IUserOrganizationRepository UserOrganizationRepository => userOrganizationRepository;
    public IUserRepository UserRepository => userRepository;
    public bool AutomaticTransactionsEnabled => transactionSettings.AutomaticTransactionsEnabled;

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A unit-of-work transaction is already active.");
        }

        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No unit-of-work transaction is active.");
        }

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No unit-of-work transaction is active.");
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();

    public IDisposable DisableAutomaticTransactions() =>
        transactionSettings.DisableAutomaticTransactions();

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
