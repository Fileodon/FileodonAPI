using Infrastructure.Models;

namespace Domain.Interface;

public interface IRoleService
{
    Task<IEnumerable<AspNetRole>> GetAllAsync(int organizationId);
    Task<AspNetRole?> GetByIdAsync(int organizationId, string id);
    Task<AspNetRole> CreateAsync(AspNetRole role);
    Task<AspNetRole?> UpdateAsync(int organizationId, string id, AspNetRole role);
    Task<bool> DeleteAsync(int organizationId, string id);
    Task<IEnumerable<long>?> GetPermissionsAsync(int orgId, string userId);
}