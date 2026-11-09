using Domain.Interface;
using Infrastructure;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Domain.Service;

public sealed class UnitOfWork : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    private readonly ApplicationDbContext _context;

    public IRepositoryTransactionSettings _transactionSettings;
    public IAuditLogRepository AuditLogRepository { get; }
    public IOrganizationRepository OrganizationRepository { get; }
    public IProfileRepository ProfileRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    public IRoleRepository RoleRepository { get; }
    public ISFTPConnectionRepository SFTPConnectionRepository { get; }
    public ISFTPConnectionAuthCodeRepository SFTPConnectionAuthCodeRepository { get; }
    public IUserOrganizationRepository UserOrganizationRepository { get; }
    public IUserRepository UserRepository { get; }
    public bool AutomaticTransactionsEnabled => _transactionSettings.AutomaticTransactionsEnabled;

    public UnitOfWork(
    ApplicationDbContext context,
    IRepositoryTransactionSettings transactionSettings,
    IAuditLogRepository auditLogRepository,
    IOrganizationRepository organizationRepository,
    IProfileRepository profileRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRoleRepository roleRepository,
    ISFTPConnectionRepository sftpConnectionRepository,
    ISFTPConnectionAuthCodeRepository sftpConnectionAuthCodeRepository,
    IUserOrganizationRepository userOrganizationRepository,
    IUserRepository userRepository)
    {
        _context = context;
        _transactionSettings = transactionSettings;
        AuditLogRepository = auditLogRepository;
        OrganizationRepository = organizationRepository;
        ProfileRepository = profileRepository;
        RefreshTokenRepository = refreshTokenRepository;
        RoleRepository = roleRepository;
        SFTPConnectionRepository = sftpConnectionRepository;
        SFTPConnectionAuthCodeRepository = sftpConnectionAuthCodeRepository;
        UserOrganizationRepository = userOrganizationRepository;
        UserRepository = userRepository;

    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A unit-of-work transaction is already active.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
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

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public IDisposable DisableAutomaticTransactions() =>
        _transactionSettings.DisableAutomaticTransactions();

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
