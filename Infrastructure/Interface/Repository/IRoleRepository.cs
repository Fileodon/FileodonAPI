using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IRoleRepository : IBaseRepository<AspNetRole>
{
    Task<AspNetRole?> GetByIdAsync(string id);
    Task<AspNetRole?> DeleteAsync(string id);
}
