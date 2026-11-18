using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class ProfileRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<Profile>(context, transactionSettings), IProfileRepository
{
}