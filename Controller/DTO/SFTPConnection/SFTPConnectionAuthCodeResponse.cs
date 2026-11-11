using Infrastructure.Models;

namespace Controller.DTO.SFTPConnection;

public class SFTPConnectionAuthCodeResponse
{
    public required string VerificationCode { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
}

public static class SFTPConnectionAuthCodeResponseExtensions
{
    public static SFTPConnectionAuthCodeResponse ToDTO(this SFTPConnectionAuthCode authCode)
    {
        return new SFTPConnectionAuthCodeResponse
        {
            VerificationCode = authCode.VerificationCode,
            OrganizationId = authCode.OrganizationId,
            UserId = authCode.UserId,
            CreatedAt = authCode.CreatedAt,
            ExpiresAt = authCode.ExpiresAt
        };
    }
}