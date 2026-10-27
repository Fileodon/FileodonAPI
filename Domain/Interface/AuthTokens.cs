namespace Domain.Interface;

public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
