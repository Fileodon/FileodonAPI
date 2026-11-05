
using Infrastructure.Interface;

namespace Infrastructure.Models;

public class BaseAuditLog
{
    public string Id { get; set; }
    public required string ActorId { get; set; }
    public User actor { get; set; } = null!;
    public string? Action { get; set; } // Enten svar overens med permissions ellers kør standart Added, Modified, Deleted
    public required string Source { get; set; } // cli or ip address 
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public DateTime Timestamp { get; set; }
}