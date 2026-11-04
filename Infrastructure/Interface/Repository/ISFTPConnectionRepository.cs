using Infrastructure.Models;
using Infrastructure.Models.Enums;

namespace Infrastructure.Interface.Repository;

public interface ISFTPConnectionRepository : IBaseRepository<SFTPConnection>
{
    Task<SFTPConnection> AddSFTPConnectionAsync(SFTPConnection connection);
    SFTPConnection AcceptSFTPConnection(SFTPConnection connection);
    SFTPConnection RejectSFTPConnection(SFTPConnection connection);
    Task<SFTPConnection?> GetSFTPConnectionByIdAsync(int sFTPConnectionId, string userId, bool noTracking = false);
    Task<SFTPConnectionStatus?> GetSFTPConnectionStatusByIdAsync(int sFTPConnectionId, string userId);
    Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId);
}