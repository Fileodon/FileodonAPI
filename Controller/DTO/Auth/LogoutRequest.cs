namespace Controller.DTO.Auth;

public record LogoutRequest
{
    public required string RefreshToken { get; init; }
}
