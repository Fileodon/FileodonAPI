using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email must be a valid email address.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.");
    }
}
