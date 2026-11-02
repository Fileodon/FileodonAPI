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
}
