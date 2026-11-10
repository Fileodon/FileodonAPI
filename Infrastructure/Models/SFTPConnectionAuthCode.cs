using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Models;

public class SFTPConnectionAuthCode
{
    [Key]
    public int Id { get; set; }
    public required string VerificationCode { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

    public virtual User? User { get; set; }
    public virtual Organization? Organization { get; set; }
}
