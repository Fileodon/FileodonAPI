using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IConfiguration configuration)
    {
        bool requireDigit = configuration.GetValue<bool>("PasswordPolicy:RequireDigit");
        bool requireLowercase = configuration.GetValue<bool>("PasswordPolicy:RequireLowercase");
        bool requireUppercase = configuration.GetValue<bool>("PasswordPolicy:RequireUppercase");
        bool requireNonAlphanumeric = configuration.GetValue<bool>("PasswordPolicy:RequireNonAlphanumeric");
        int minPasswordLength = configuration.GetValue<int>("PasswordPolicy:RequiredLength");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email must not be empty.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username must not be empty.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(20).WithMessage("Username must not exceed 20 characters.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.")
            .MinimumLength(minPasswordLength).WithMessage($"Password must be at least {minPasswordLength} characters long.")
            .Matches(requireDigit ? @"\d" : ".*").WithMessage("Password must contain at least one digit.")
            .Matches(requireLowercase ? @"[a-z]" : ".*").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(requireUppercase ? @"[A-Z]" : ".*").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(requireNonAlphanumeric ? @"\W" : ".*").WithMessage("Password must contain at least one non-alphanumeric character.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}
