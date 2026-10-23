namespace Infrastructure.Models;

public partial class Organization
{
    public int Id { get; set; }

    public string OrgName { get; set; } = null!;

    public ICollection<SFTPConnection> SFTPConnections { get; set; } = [];
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = [];
}
