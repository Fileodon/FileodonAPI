using System.Linq.Expressions;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Interface.Repository;

public interface IUserOrganizationRepository : IBaseRepository<UserOrganization>
{
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId);
    Task<IEnumerable<UserOrganization>> GetUserOrganizationsByOrganizationIdAsync(int organizationId);
    Task<IEnumerable<UserOrganization>> GetUserOrganizationsByUseridAsync(string userId);
    Task<UserOrganization?> GetByIdWithRelationshipsAsync(int id);
    Task<IQueryable<UserOrganization>> WhereAsync(Expression<Func<UserOrganization, bool>> predicate, Func<IQueryable<UserOrganization>, IQueryable<UserOrganization>>? includes = null);
}