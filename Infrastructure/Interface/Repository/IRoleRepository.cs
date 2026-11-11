using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IRoleRepository : IBaseRepository<AspNetRole>
{
    Task<AspNetRole?> GetByIdAsync(string id);
    Task<AspNetRole?> DeleteAsync(string id);
    Task<IEnumerable<AspNetRole>> GetAllRolesAndOrganizations();
    Task<AspNetRole?> GetRoleAndOrganizations(string id);
    Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsAsync(string roleId);
    Task<IEnumerable<long>?> GetPermissionsAsync(int orgId, string userId);
}
