namespace Infrastructure.Models;

public class SFTPActionAuditLog : BaseAuditLog
{
    public required int SFTPConnectionID { get; set; }
    public virtual SFTPConnection SFTPConnection { get; set; } = null!;
}
