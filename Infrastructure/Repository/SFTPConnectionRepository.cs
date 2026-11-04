using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
using Infrastructure.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class SFTPConnectionRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<SFTPConnection>(context, transactionSettings), ISFTPConnectionRepository
{
    private readonly ApplicationDbContext _context = context;
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

    public async Task<SFTPConnection?> GetSFTPConnectionByIdAsync(int sFTPConnectionId, string userId, bool noTracking = false)
    {
        IQueryable<SFTPConnection> query = _context.SFTPConnections
            .Where(c => c.Id == sFTPConnectionId &&
                c.Organization.UserOrganizations.Any(uo => uo.UserId == userId));

        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<SFTPConnectionStatus?> GetSFTPConnectionStatusByIdAsync(int sFTPConnectionId, string userId)
    {
        return await _context.SFTPConnections
            .Where(c => c.Id == sFTPConnectionId &&
                c.Organization.UserOrganizations.Any(uo => uo.UserId == userId))
            .Select(c => (SFTPConnectionStatus?)c.Status)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId)
    {
        return await _context.SFTPConnections
            .Where(c => c.OrganizationId == organizationId &&
                c.Organization.UserOrganizations.Any(uo => uo.UserId == userId)).ToListAsync();
    }
}