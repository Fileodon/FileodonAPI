using System;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class SFTPActionAuditLogRepository : BaseRepository<SFTPActionAuditLog>, ISFTPActionAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public SFTPActionAuditLogRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : base(context, transactionSettings)
    {
        _context = context;
    }

    public async Task<SFTPActionAuditLog> AddSFTPActionAuditLog(SFTPActionAuditLog sftpActionAuditLog)
    {
        await _context.SFTPActionAuditLogs.AddAsync(sftpActionAuditLog);
        await _context.SaveChangesAsync();
        return sftpActionAuditLog;
    }

    public Task<IEnumerable<SFTPActionAuditLog>> GetSFTPActionAuditLogsAsync(int sFTPconnectionId)
    {
        return Task.FromResult(_context.SFTPActionAuditLogs
            .Where(audit => audit.SFTPConnectionID == sFTPconnectionId)
            .AsEnumerable());
    }
}
