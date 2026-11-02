using Domain.DTO;
using Infrastructure.Models;

namespace Domain.Interface;

public interface ISFTPConnectionService
{
    public Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId);
}
