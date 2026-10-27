namespace Infrastructure.Models;

public partial class UserOrganization
{
    public int Id { get; set; }

    //FK
    public string? UserId { get; set; }
    public string RoleId { get; set; } = null!;
    public int? OrganizationId { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual User? User { get; set; }
    public virtual AspNetRole Role { get; set; } = null!;
}
