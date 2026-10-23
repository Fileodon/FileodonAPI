using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class UserRepository(ApplicationDbContext context) : BaseRepository<User>(context), IUserRepository
{
}