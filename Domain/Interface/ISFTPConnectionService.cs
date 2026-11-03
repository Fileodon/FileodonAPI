using Domain.DTO;
using Infrastructure.Models;

namespace Domain.Interface;

public interface ISFTPConnectionService
{
    public Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId);
    public Task<MutationResult<SFTPConnection>> AddSFTPConnectionAsync(string authCode, string userId, string displayName, string host, int port);
}
