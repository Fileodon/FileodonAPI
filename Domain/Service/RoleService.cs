using Infrastructure.Models;
using Domain.Interface;
namespace Domain.Service
{
    public class RoleService : IRoleService<AspNetRole>
    {

        public async Task<IEnumerable<AspNetRole>> GetAllAsync()
        {       
            return await Task.FromResult(new List<AspNetRole>());
        }
        public async 
    }
}
