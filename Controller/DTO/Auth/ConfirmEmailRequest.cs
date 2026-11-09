namespace Controller.DTO.Auth;

public record class ConfirmEmailRequest
{
    public required string UserId { get; init; }
    public required string Token { get; init; }
}
