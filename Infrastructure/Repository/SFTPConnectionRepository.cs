using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class SFTPConnectionRepository : BaseRepository<SFTPConnection>, ISFTPConnectionRepository
{
    private readonly ApplicationDbContext _context;

    public SFTPConnectionRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    public async Task<SFTPConnection> AddSFTPConnectionAsync(SFTPConnection connection)
    {
        await _context.SFTPConnections.AddAsync(connection);
        return connection;
    }

    public SFTPConnection AcceptSFTPConnection(SFTPConnection connection)
    {
        connection.Status = SFTPConnectionStatus.Accepted;
        _context.SFTPConnections.Update(connection);
        return connection;
    }

    public SFTPConnection RejectSFTPConnection(SFTPConnection connection)
    {
        connection.Status = SFTPConnectionStatus.Rejected;
        _context.SFTPConnections.Update(connection);
        return connection;
    }

    /// <summary>
    /// Get SFTP Connection by Id for a specific organization and user. Returns null if not found or if the user does not have access to the organization.
    /// </summary>
    /// <param name="sFTPConnectionId"></param>
    /// <param name="organizationId"></param>
    /// <param name="userId"></param>
    /// <param name="noTracking">Whether to use no tracking for the query. Default is false.</param>
    /// <returns></returns>
    public async Task<SFTPConnection?> GetSFTPConnectionByIdAsync(int sFTPConnectionId, int organizationId, string userId, bool noTracking = false)
    {
        IQueryable<SFTPConnection> query = _context.SFTPConnections
            .Where(c => c.Id == sFTPConnectionId &&
                c.Organization.Id == organizationId &&
                c.Organization.UserOrganizations.Any(uo => uo.UserId == userId));

        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get the status of an SFTP Connection by Id for a specific organization and user. Returns null if not found or if the user does not have access to the organization.
    /// </summary>
    /// <param name="sFTPConnectionId"></param>
    /// <param name="organizationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<SFTPConnection?> GetSFTPConnectionStatusByIdAsync(int sFTPConnectionId, int organizationId)
    {
        return await _context.SFTPConnections
            .Where(c => c.Id == sFTPConnectionId &&
                c.Organization.Id == organizationId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId)
    {
        return await _context.SFTPConnections
            .Where(c => c.OrganizationId == organizationId &&
                c.Organization.UserOrganizations.Any(uo => uo.UserId == userId)).ToListAsync();
    }
    public async Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogsAsync(int sftpConnectionId)
    {
        return _context.SFTPConnectionAuditLogs
            .Where(audit => audit.SFTPConnectionID == sftpConnectionId);
    }
}