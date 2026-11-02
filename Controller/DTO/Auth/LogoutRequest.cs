namespace Controller.DTO.Auth;

public record class LogoutRequest
{
    public required string RefreshToken { get; init; }
}
