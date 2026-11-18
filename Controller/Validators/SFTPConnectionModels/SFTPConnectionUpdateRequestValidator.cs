using Controller.DTO.SFTPConnection;
using FluentValidation;

namespace Controller.Validators.SFTPConnectionModels;

public class SFTPConnectionUpdateRequestValidator : AbstractValidator<SFTPConnectionUpdateRequest>
{
    public SFTPConnectionUpdateRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .Length(5, 100)
            .WithMessage("DisplayName must be between 5 and 100 characters long.");
    }
}
