using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Models;

public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = [];
}