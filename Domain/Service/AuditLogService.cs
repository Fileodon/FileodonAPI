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
        var orgAuditLogs = await _unitOfWork.AuditLogRepository.GetOrganizationAuditLogs(id);
        
        return new AllAuditLogsServiceReturn
        {
            organizationAuditLogs = orgAuditLogs
        };
    }
}
