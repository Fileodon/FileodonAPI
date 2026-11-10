using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Interface.Repository;

public interface IUserOrganizationRepository : IBaseRepository<UserOrganization>
{
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId);
    Task<IQueryable<UserOrganization>> WhereAsync(
    Expression<Func<UserOrganization, bool>> predicate,
    Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null);
}