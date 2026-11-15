using Domain.DTO;
using Infrastructure.Models;
using Infrastructure.Models.Enums;

namespace Domain.Interface;

public interface ISFTPConnectionService
{
    public Task<MutationEnvelope<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId);
    public Task<MutationEnvelope<SFTPConnection>> AddSFTPConnectionAsync(string authCode, int organizationId, string? displayName, string host, int? port);
    public Task<MutationEnvelope<SFTPConnectionAuthCode>> GetSFTPConnectionAuthCodeAsync(string authCode);
    public Task<MutationEnvelope<SFTPConnection>> UpdateSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId, SFTPConnectionStatus newStatus);
    public Task<MutationEnvelope<SFTPConnection>> PollSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId);
    public Task<MutationEnvelope<SFTPConnection>> GetSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId);
    public Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId);
    public Task<MutationEnvelope<SFTPConnection>> DeleteSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId);
    public Task<MutationEnvelope<SFTPConnection>> UpdateSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId, string displayName);
}
