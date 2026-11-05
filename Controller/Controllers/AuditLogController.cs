using Domain.Service;

namespace Controller.Controllers;

public class AuditLogController
{
    private readonly UnitOfWork _unitOfWork;
    public AuditLogController(UnitOfWork unitOfWork){
        _unitOfWork = unitOfWork;
    }
}