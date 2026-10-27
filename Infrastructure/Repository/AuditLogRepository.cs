using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class AuditLogRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<AuditLog>(context, transactionSettings), IAuditLogRepository
{
}