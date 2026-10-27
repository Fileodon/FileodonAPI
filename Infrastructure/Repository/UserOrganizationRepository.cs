using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class UserOrganizationRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<UserOrganization>(context, transactionSettings), IUserOrganizationRepository
{
}