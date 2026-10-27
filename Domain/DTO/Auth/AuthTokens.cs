namespace Domain.DTO.Auth;

public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
