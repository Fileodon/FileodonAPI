using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class SFTPConnectionRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<SFTPConnection>(context, transactionSettings), ISFTPConnectionRepository
{
}