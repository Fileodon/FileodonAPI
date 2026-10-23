using Infrastructure.Models;

namespace Domain.Interface;

public interface ISFTPConnectionService
{
    public Task<SFTPConnectionAuthCode> GenerateVerificationCodeAsync(int organizationId, string userId);
}
