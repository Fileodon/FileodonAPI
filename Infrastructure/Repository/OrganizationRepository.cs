using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class OrganizationRepository(ApplicationDbContext context) : BaseRepository<Organization>(context), IOrganizationRepository
{
}