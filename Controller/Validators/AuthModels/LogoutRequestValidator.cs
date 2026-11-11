using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token must not be empty.");
    }
}
