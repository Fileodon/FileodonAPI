using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email must not be empty.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username must not be empty.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty."); // Vi validere om password følger regler med Identity senere
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
