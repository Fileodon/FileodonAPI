using Infrastructure.Models;

namespace Domain.Interface;

public interface IRoleService
{
    Task<IEnumerable<AspNetRole>> GetAllAsync();
    Task<AspNetRole?> GetByIdAsync(string id);
    Task<AspNetRole> CreateAsync(AspNetRole role);
    Task<AspNetRole?> UpdateAsync(string id, AspNetRole role);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<long>?> GetPermissionsAsync(int orgId, string userId);
}