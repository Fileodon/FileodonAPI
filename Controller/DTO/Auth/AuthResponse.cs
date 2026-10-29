namespace Controller.DTO.Auth;

/// <summary>
/// Contains an access token and its rotating refresh token.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    long RefreshTokenExpiresAt);
