namespace Controller.DTO.Auth;

public record class ResetPasswordRequest(
    string OldPassword,
    string NewPassword
);
