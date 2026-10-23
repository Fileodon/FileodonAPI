using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class OrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<Organization>(context, transactionSettings), IOrganizationRepository
{
}