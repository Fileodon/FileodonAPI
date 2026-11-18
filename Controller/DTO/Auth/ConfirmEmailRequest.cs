namespace Controller.DTO.Auth;

public record class ConfirmEmailRequest(
    string UserId,
    string Token
);
