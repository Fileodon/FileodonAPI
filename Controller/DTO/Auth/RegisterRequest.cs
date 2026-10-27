namespace Controller.DTO.Auth;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password
);
