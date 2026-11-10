namespace Controller.DTO.Auth;

public record ResetPasswordRequest(
    string OldPassword,
    string NewPassword
);
