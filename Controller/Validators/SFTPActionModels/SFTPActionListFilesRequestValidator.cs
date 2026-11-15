using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionListFilesRequestValidator : AbstractValidator<SFTPActionListFilesRequest>
{
    public SFTPActionListFilesRequestValidator()
    {
        RuleFor(x => x.RemoteDirectoryPath)
            .NotEmpty().WithMessage("Remote directory path is required.")
            .Must(CommonRules.BeAValidPath).WithMessage("Remote directory path must be a valid path.");
    }
}
