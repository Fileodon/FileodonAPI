using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class UserOrganizationRepository(ApplicationDbContext context) : BaseRepository<UserOrganization>(context), IUserOrganizationRepository
{
}