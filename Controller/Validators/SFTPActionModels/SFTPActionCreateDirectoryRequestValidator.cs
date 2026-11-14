using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionCreateDirectoryRequestValidator : AbstractValidator<SFTPActionCreateDirectoryRequest>
{
    public SFTPActionCreateDirectoryRequestValidator()
    {
        RuleFor(x => x.RemoteDirectoryPath)
            .NotEmpty().WithMessage("Directory path is required.")
            .Must(CommonRules.BeAValidPath).WithMessage("Directory path is not valid.");
    }
}
