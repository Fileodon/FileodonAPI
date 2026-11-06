using Microsoft.AspNetCore.Identity;
using Infrastructure.Interface;
namespace Infrastructure.Models;

public class AspNetRole : IdentityRole, IAuditAble
{
    public int OrganizationId { get; set; }
    public long PermissionMask { get; set; }

    public virtual Organization Organization { get; set; } = null!;
}
