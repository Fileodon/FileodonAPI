using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class AuditLogRepository(ApplicationDbContext context) : BaseRepository<AuditLog>(context), IAuditLogRepository
{
}