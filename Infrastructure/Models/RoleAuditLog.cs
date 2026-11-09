namespace Infrastructure.Models;

public class RoleAuditLog: BaseAuditLog
{
    public string RoleId { get; set; } = null!;
    public virtual AspNetRole Role { get; set; } = null!;
}