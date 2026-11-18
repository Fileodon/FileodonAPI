using System;
using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface ISFTPActionAuditLogRepository
{
    Task<IEnumerable<SFTPActionAuditLog>> GetSFTPActionAuditLogsAsync(int organizationId);
    Task<SFTPActionAuditLog> AddSFTPActionAuditLog(SFTPActionAuditLog sftpActionAuditLog);
}
