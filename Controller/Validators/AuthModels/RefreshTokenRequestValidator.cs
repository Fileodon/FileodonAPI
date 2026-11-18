using Controller.DTO.Auth;
using FluentValidation;

namespace Controller.Validators.AuthModels;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token must not be empty.");
    }
}
