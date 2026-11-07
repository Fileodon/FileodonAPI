using Domain.Interface;
using Infrastructure.Models;

namespace Domain.Service;


public class AuditLogService: IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AllAuditLogsServiceReturn> GetAllAuditLogsInOrganization(int id)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var orgAuditLogs = await _unitOfWork.AuditLogRepository.GetOrganizationAuditLogsFromOrganizationId(id);
            var sftpConnectionAuditLog = await _unitOfWork.AuditLogRepository.GetSFTPConnectionAuditLogsFromOrganizationId(id);
            var userOrgAuditLogs = await _unitOfWork.AuditLogRepository.GetUserOrganizationAuditLogsFromOrganizationId(id);
            var roleAuditLogs = await _unitOfWork.AuditLogRepository.GetRoleAuditLogsFromOrganizationId(id);
            return new AllAuditLogsServiceReturn
            {
                organizationAuditLogs = orgAuditLogs,
                SFTPConnectionAuditLogs = sftpConnectionAuditLog,
                RoleAuditLogs = roleAuditLogs,
                UserOrganizationAuditLogs = userOrgAuditLogs
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
