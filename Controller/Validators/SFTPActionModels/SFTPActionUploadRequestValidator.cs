using System;
using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionUploadRequestValidator : AbstractValidator<SFTPActionUploadRequest>
{
    public SFTPActionUploadRequestValidator()
    {
        RuleFor(x => x.RemoteFilePath).NotEmpty().WithMessage("Remote file path is required.")
            .Must(HaveAFileName).WithMessage("Remote file path must have a valid file name.");
        RuleFor(x => x.File).NotNull().WithMessage("File is required.")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty.");
    }
    private static bool HaveAFileName(string filename)
    {
        if (string.IsNullOrEmpty(Path.GetExtension(filename)) ||
            string.IsNullOrEmpty(Path.GetFileName(filename)))
        {
            return false;
        }

        return true;
    }
}
