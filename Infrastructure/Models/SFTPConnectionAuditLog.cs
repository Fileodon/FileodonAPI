namespace Infrastructure.Models;

public class SFTPConnectionAuditLog : BaseAuditLog
{
    public int SFTPConnectionID { get; set; }
    public virtual SFTPConnection SFTPConnnection { get; set; } = null!;
}