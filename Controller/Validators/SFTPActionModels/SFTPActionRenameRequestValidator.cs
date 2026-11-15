using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionRenameRequestValidator : AbstractValidator<SFTPActionRenameRequest>
{
    public SFTPActionRenameRequestValidator()
    {
        RuleFor(x => x.OldRemoteFilePath)
            .NotEmpty().WithMessage("Old remote file path is required.")
            .Must(CommonRules.BeAValidPath).WithMessage("Old remote file path must be a valid path.");
        RuleFor(x => x.NewRemoteFilePath)
            .NotEmpty().WithMessage("New remote file path is required.")
            .Must(CommonRules.BeAValidPath).WithMessage("New remote file path must be a valid path.");
    }
}
