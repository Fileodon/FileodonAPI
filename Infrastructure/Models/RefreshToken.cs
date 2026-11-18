namespace Infrastructure.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public required string UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}
