using Domain.Interface;

namespace Domain.Service;


public class AuditLogService: IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    
}