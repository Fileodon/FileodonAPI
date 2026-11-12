using System;
using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionListFilesRequestValidator : AbstractValidator<SFTPActionListFilesRequest>
{
    public SFTPActionListFilesRequestValidator()
    {
        RuleFor(x => x.RemoteDirectoryPath)
            .NotEmpty().WithMessage("Remote directory path is required.")
            .Must(BeAValidPath).WithMessage("Remote directory path must be a valid path.");
    }

    private static bool BeAValidPath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return !string.IsNullOrEmpty(fullPath);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
