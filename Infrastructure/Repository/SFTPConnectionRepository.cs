using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;
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
}