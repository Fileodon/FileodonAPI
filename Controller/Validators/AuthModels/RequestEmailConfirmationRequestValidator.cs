using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class RequestEmailConfirmationRequestValidator : AbstractValidator<RequestEmailConfirmationRequest>
{
    public RequestEmailConfirmationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
