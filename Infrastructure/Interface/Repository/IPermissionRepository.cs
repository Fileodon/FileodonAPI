using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

internal interface IPermissionRepository : IBaseRepository<Permission>
{
    Task<IEnumerable<Permission>> GetPermissionsAsync(string userId);
}
