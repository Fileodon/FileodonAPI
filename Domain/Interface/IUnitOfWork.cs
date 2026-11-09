using Infrastructure.Interface;
using Infrastructure.Interface.Repository;

namespace Domain.Interface;

/// <summary>
/// Defines a unit of work pattern interface that provides access to repositories and manages database transactions.
/// Implements IDisposable to ensure proper resource cleanup.
/// </summary>
/// <remarks>
/// The Unit of Work pattern maintains a list of objects affected by a business transaction and 
/// coordinates writing out changes and resolving concurrency problems.
/// </remarks>
public interface IUnitOfWork : IDisposable
{
    IOrganizationRepository OrganizationRepository { get; }
    IProfileRepository ProfileRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IRoleRepository RoleRepository { get; }
    ISFTPConnectionRepository SFTPConnectionRepository { get; }
    ISFTPConnectionAuthCodeRepository SFTPConnectionAuthCodeRepository { get; }
    IUserOrganizationRepository UserOrganizationRepository { get; }
    IAuditLogRepository AuditLogRepository { get; }
    IUserRepository UserRepository { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
    bool AutomaticTransactionsEnabled { get; }
    IDisposable DisableAutomaticTransactions();
}