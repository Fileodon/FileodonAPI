using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class RolesOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<RolesOrganization>(context, transactionSettings), IRolesOrganizationRepository
{
}
