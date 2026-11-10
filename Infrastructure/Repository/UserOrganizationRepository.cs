using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repository;

public class UserOrganizationRepository : BaseRepository<UserOrganization>, IUserOrganizationRepository
{
    private readonly ApplicationDbContext _context;
    public UserOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }
    public Task<IQueryable<UserOrganization>> WhereAsync(
        Expression<Func<UserOrganization, bool>> predicate,
        Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<UserOrganization> query = _context
            .Set<UserOrganization>()
            .Where(predicate);

        if (includes is not null)
        {
            query = includes(query);
        }

        return Task.FromResult(query);
    }

    public async Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId)
    {
        return _context.UserOrganizationAuditLogs
            .Where(audit => audit.UserOrganizationId == userOrganizationId);
    }
}