namespace Infrastructure.Models;

public class UserOrganizationAuditLog: BaseAuditLog
{
    public int UserOrganizationId { get; set; }
    public virtual UserOrganization UserOrganization { get; set; } = null!; 
}