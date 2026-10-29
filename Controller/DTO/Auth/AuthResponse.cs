namespace Controller.DTO.Auth;

/// <summary>
/// Contains an access token and the expiration time of the refresh token in Unix time seconds.
/// </summary>
public record AuthResponse(
    string AccessToken,
    long RefreshTokenExpiryTimestamp);
