using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface ISFTPConnectionRepository : IBaseRepository<SFTPConnection>
{
    Task<SFTPConnection> AddSFTPConnectionAsync(SFTPConnection connection);
    SFTPConnection AcceptSFTPConnection(SFTPConnection connection);
    SFTPConnection RejectSFTPConnection(SFTPConnection connection);
    Task<SFTPConnection?> GetSFTPConnectionByIdAsync(int sFTPConnectionId, int organizationId, string userId, bool noTracking = false);
    Task<SFTPConnection?> GetSFTPConnectionStatusByIdAsync(int sFTPConnectionId, int organizationId, string userId);
    Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId);
    Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsAsync(int sftpConnectionId);
}