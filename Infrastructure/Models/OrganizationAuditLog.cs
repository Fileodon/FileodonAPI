using Infrastructure.Interface;

namespace Infrastructure.Models;

public class OrganizationAuditLog: BaseAuditLog, IAuditLog
{
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
}