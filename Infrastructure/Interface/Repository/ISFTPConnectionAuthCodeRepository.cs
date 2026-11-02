using System;
using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface ISFTPConnectionAuthCodeRepository : IBaseRepository<SFTPConnectionAuthCode>
{
    Task<SFTPConnectionAuthCode?> AddAuthCodeAsync(string authCode, int organizationId, string userId);
}
