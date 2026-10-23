using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class UserRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<User>(context, transactionSettings), IUserRepository
{
}