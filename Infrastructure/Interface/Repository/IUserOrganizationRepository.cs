using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IUserOrganizationRepository : IBaseRepository<UserOrganization>
{
    Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogsAsync(int userOrganizationId);
    Task<IEnumerable<UserOrganization>> GetUserOrganizationsByOrganizationIdAsync(int organizationId);
    Task<IEnumerable<UserOrganization>> GetUserOrganizationsByUseridAsync(string userId);
    Task<UserOrganization?> GetByIdWithRelationshipsAsync(int id);
}