namespace Infrastructure.Models;

public class OrganizationAuditLog : BaseAuditLog
{
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
}