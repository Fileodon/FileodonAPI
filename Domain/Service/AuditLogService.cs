using Domain.Interface;
using Infrastructure.Models;

namespace Domain.Service;


public class AuditLogService(IUnitOfWork unitOfWork) : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

    public async Task<IEnumerable<OrganizationAuditLog>> GetOrganizationAuditLogs(int id)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            return await _unitOfWork.OrganizationRepository.GetOrganizationAuditLogsAsync(id);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task<IEnumerable<SFTPConnectionAuditLog>> GetSFTPConnectionAuditLogs(int id)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            return await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionAuditLogsAsync(id);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogs(string id)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            return await _unitOfWork.RoleRepository.GetRoleAuditLogsAsync(id);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    public async Task<IEnumerable<UserOrganizationAuditLog>> GetUserOrganizationAuditLogs(int id)
    {
        using IDisposable transactionScope = _unitOfWork.DisableAutomaticTransactions();
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            return await _unitOfWork.UserOrganizationRepository.GetUserOrganizationAuditLogsAsync(id);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
