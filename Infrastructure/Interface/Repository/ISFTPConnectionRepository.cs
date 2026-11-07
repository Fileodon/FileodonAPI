using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface ISFTPConnectionRepository : IBaseRepository<SFTPConnection>
{
    Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsAsync(int sftpConnectionId);
}