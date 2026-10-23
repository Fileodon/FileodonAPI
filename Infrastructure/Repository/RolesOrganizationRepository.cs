using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class RolesOrganizationRepository(ApplicationDbContext context) : BaseRepository<RolesOrganization>(context), IRolesOrganizationRepository
{
}
