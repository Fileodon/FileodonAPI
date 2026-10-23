using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class RoleRepository(ApplicationDbContext context) : BaseRepository<AspNetRole>(context), IRoleRepository
{
}
