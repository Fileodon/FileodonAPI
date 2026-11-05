using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Models;

public partial class Organization
{
    public int Id { get; set; }

    [ForeignKey(nameof(UserOwner))]
    public required string UserOwnerId { get; set; }
    public required string OrgName { get; set; }
    public required string WorkspaceURL { get; set; }
    public required string ContactEmail { get; set; }

    public virtual User? UserOwner { get; set; }
    public ICollection<SFTPConnection> SFTPConnections { get; set; } = [];
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = [];
    public virtual ICollection<OrganizationAuditLog> AuditLogs { get; set; } = [];
}
