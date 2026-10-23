using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class ProfileRepository(ApplicationDbContext context) : BaseRepository<Profile>(context), IProfileRepository
{
}