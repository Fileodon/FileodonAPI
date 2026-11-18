using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IRoleRepository : IBaseRepository<AspNetRole>
{
    Task<AspNetRole?> GetByIdAsync(int organizationId,string id);
    Task<AspNetRole?> DeleteAsync(int organizationId, string id);
    Task<IEnumerable<AspNetRole>> GetAllRolesInOrganization(int organizationId);
    Task<AspNetRole?> GetRoleAndOrganizations(string id);
    Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsAsync(string roleId);
    Task<IEnumerable<long>?> GetPermissionsAsync(int organizationId, string userId);
}
