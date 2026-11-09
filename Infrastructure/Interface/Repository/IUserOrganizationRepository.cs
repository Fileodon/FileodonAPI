using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IUserOrganizationRepository : IBaseRepository<UserOrganization>
{
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId);
}