using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class SFTPConnectionAuthCodeRepository : BaseRepository<SFTPConnectionAuthCode>, ISFTPConnectionAuthCodeRepository
{
    private readonly ApplicationDbContext _context;

    public SFTPConnectionAuthCodeRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    public async Task<SFTPConnectionAuthCode?> AddAuthCodeAsync(string authCode, int organizationId, string userId)
    {
        Organization? organization = await _context.Organizations
            .Where(o =>
                o.Id == organizationId &&
                o.UserOrganizations.Any(uo => uo.UserId == userId))
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            return null;
        }

        var sftpConnectionAuthCode = new SFTPConnectionAuthCode
        {
            OrganizationId = organizationId,
            UserId = userId,
            VerificationCode = authCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _context.SFTPConnectionAuthCodes.AddAsync(sftpConnectionAuthCode);
        return sftpConnectionAuthCode;
    }

    /// <summary>
    /// Retrieves an SFTPConnectionAuthCode entity based on the provided authCode and organizationId.
    /// </summary>
    /// <param name="authCode"></param>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    public Task<SFTPConnectionAuthCode?> GetAuthCodeAsync(string authCode, int organizationId)
    {
        return _context.SFTPConnectionAuthCodes
            .Where(ac =>
                string.Equals(ac.VerificationCode, authCode, StringComparison.OrdinalIgnoreCase) &&
                ac.Organization!.Id == organizationId)
            .FirstOrDefaultAsync();
    }
}
