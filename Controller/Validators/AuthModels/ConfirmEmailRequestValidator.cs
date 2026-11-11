using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(BeAValidGuid).WithMessage("UserId must be a valid GUID.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }

    private bool BeAValidGuid(string arg)
    {
        return Guid.TryParse(arg, out _);
    }
}
