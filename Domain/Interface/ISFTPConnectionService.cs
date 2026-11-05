using Domain.DTO;
using Infrastructure.Models;
using Infrastructure.Models.Enums;

namespace Domain.Interface;

public interface ISFTPConnectionService
{
    public Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId);
    public Task<MutationResult<SFTPConnection>> AddSFTPConnectionAsync(string authCode, int organizationId, string displayName, string host, int port);
    public Task<MutationResult<SFTPConnection>> UpdateSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId, SFTPConnectionStatus newStatus);
    public Task<MutationResult<SFTPConnection>> PollSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId);
    public Task<MutationResult<SFTPConnection>> GetSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId);
    public Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId);
    public Task<MutationResult<SFTPConnection>> DeleteSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId);
}
