using Infrastructure.Models.Enums;

namespace Infrastructure.Models;

public sealed class SFTPDistributionJob
{
    public Guid Id { get; set; }
    public int OrganizationId { get; set; }
    public required string UserId { get; set; }
    public SFTPDistributionJobStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
    public int TotalFiles { get; set; }
    public long TotalBytes { get; set; }
    public required string FilesJson { get; set; }
    public required string DestinationsJson { get; set; }

    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
}
