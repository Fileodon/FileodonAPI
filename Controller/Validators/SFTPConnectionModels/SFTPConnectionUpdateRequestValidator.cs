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
        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .When(x => x.Port.HasValue)
            .WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.IntermediaryPort)
            .InclusiveBetween(1, 65535)
            .When(x => x.IntermediaryPort.HasValue)
            .WithMessage("IntermediaryPort must be between 1 and 65535.");
    }
}
