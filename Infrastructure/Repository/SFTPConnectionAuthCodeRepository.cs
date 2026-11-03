using System;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class SFTPConnectionAuthCodeRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<SFTPConnectionAuthCode>(context, transactionSettings), ISFTPConnectionAuthCodeRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<SFTPConnectionAuthCode?> AddAuthCodeAsync(string authCode, int organizationId, string userId)
    {
        Organization? organization = await _context.Organizations
            .Include(o => o.UserOrganizations)
            .ThenInclude(uo => uo.User)
            .FirstOrDefaultAsync(o => o.Id == organizationId && o.UserOrganizations.Any(uo => uo.UserId == userId));

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
    /// Retrieves an SFTPConnectionAuthCode by its verification code and user ID, including the associated organization and user details.
    /// </summary>
    /// <param name="authCode"></param>
    /// <param name="userId"></param>
    /// <returns>>An SFTPConnectionAuthCode if found; otherwise, null.</returns>
    /// <remarks>
    /// This method ensures that the retrieved SFTPConnectionAuthCode is associated with an organization that
    /// has a UserOrganization entry for the specified user ID. It includes the related Organization and User details in the result.
    /// </remarks>
    public Task<SFTPConnectionAuthCode?> GetAuthCodeAsync(string authCode, string userId)
    {
        return _context.SFTPConnectionAuthCodes
            .Include(ac => ac.Organization)
            .ThenInclude(o => o!.UserOrganizations)
            .ThenInclude(uo => uo.User)
            .FirstOrDefaultAsync(ac =>
                ac.VerificationCode == authCode &&
                ac.Organization != null &&
                ac.Organization.UserOrganizations.Any(uo => uo.UserId == userId));
    }
}
