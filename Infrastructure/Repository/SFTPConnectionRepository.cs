using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class SFTPConnectionRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<SFTPConnection>(context, transactionSettings), ISFTPConnectionRepository
{
    private readonly ApplicationDbContext _context = context;
    public async Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsAsync(int sftpConnectionId)
    {
        return _context.SFTPConnectionAuditLogs
            .Where(audit => audit.SFTPConnectionID == sftpConnectionId);
    }
}