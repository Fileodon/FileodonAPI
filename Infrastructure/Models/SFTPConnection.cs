using Infrastructure.Interface;
using Infrastructure.Models.Enums;

namespace Infrastructure.Models;

public partial class SFTPConnection : IAuditAble
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public int OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public required string Host { get; set; }
    public int Port { get; set; }
    public SFTPConnectionStatus Status { get; set; }
    public string? PublicKey { get; set; }
    public string? PrivateKey { get; set; }
    public virtual ICollection<SFTPConnectionAuditLog> AuditLogs { get; set; } = [];

}
