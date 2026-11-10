using Infrastructure.Interface;
using Microsoft.AspNetCore.Identity;
namespace Infrastructure.Models;

public class AspNetRole : IdentityRole, IAuditAble
{
    public int OrganizationId { get; set; }
    public long PermissionMask { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<RoleAuditLog> AuditLogs { get; set; } = [];

}
