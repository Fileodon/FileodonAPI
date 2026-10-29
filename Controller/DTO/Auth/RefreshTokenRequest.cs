namespace Controller.DTO.Auth;

/// <summary>
/// Request containing a refresh token to exchange for a new token pair.
/// </summary>
public sealed record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
