using Infrastructure.Models;

namespace Domain.Interface
{
    public interface IPermissionService : IBaseInterface<Permission>
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync(string userId);
    }
}
