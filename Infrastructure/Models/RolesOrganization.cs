using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models;

public partial class RolesOrganization
{
    public int Id { get; set; }
    public string RoleId { get; set; } = null!;
    public int OrganizationId { get; set; }

    public virtual IdentityRole Role { get; set; } = null!;
    public virtual Organization Organization { get; set; } = null!;
}
