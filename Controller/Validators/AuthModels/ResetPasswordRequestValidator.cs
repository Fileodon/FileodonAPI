using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(BeAValidGuid).WithMessage("UserId must be a valid GUID.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required."); // Vi validere om password følger regler med Identity senere
    }

    private bool BeAValidGuid(string arg)
    {
        return Guid.TryParse(arg, out _);
    }
}