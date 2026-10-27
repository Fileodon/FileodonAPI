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
    IAuditLogRepository AuditLogRepository { get; }
    IOrganizationRepository OrganizationRepository { get; }
    IProfileRepository ProfileRepository { get; }
    IRoleRepository RoleRepository { get; }
    IRolesOrganizationRepository RolesOrganizationRepository { get; }
    ISFTPConnectionRepository SFTPConnectionRepository { get; }
    IUserOrganizationRepository UserOrganizationRepository { get; }
    IUserRepository UserRepository { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync();
    bool AutomaticTransactionsEnabled { get; }
    IDisposable DisableAutomaticTransactions();
}