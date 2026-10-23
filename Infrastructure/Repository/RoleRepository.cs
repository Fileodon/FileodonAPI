using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class RoleRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<AspNetRole>(context, transactionSettings), IRoleRepository
{
}
