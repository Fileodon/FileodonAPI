using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator(IConfiguration configuration)
    {
        bool requireDigit = configuration.GetValue<bool>("PasswordPolicy:RequireDigit");
        bool requireLowercase = configuration.GetValue<bool>("PasswordPolicy:RequireLowercase");
        bool requireUppercase = configuration.GetValue<bool>("PasswordPolicy:RequireUppercase");
        bool requireNonAlphanumeric = configuration.GetValue<bool>("PasswordPolicy:RequireNonAlphanumeric");
        int minPasswordLength = configuration.GetValue<int>("PasswordPolicy:RequiredLength");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(BeAValidGuid).WithMessage("UserId must be a valid GUID.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required.")
            .MinimumLength(minPasswordLength).WithMessage($"Password must be at least {minPasswordLength} characters long.")
            .Matches(requireDigit ? @"\d" : ".*").WithMessage("Password must contain at least one digit.")
            .Matches(requireLowercase ? @"[a-z]" : ".*").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(requireUppercase ? @"[A-Z]" : ".*").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(requireNonAlphanumeric ? @"\W" : ".*").WithMessage("Password must contain at least one non-alphanumeric character.");
    }

    private bool BeAValidGuid(string arg)
    {
        return Guid.TryParse(arg, out _);
    }
}