using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionUploadRequestValidator : AbstractValidator<SFTPActionUploadRequest>
{
    public SFTPActionUploadRequestValidator()
    {
        RuleFor(x => x.Files).NotEmpty().WithMessage("At least one file is required.")
            .Must(files => files is not null && files.All(file => file is not null && file.Length > 0))
            .WithMessage("Files cannot be empty.");
        RuleFor(x => x.RemoteFilePaths).NotEmpty().WithMessage("At least one remote file path is required.")
            .Must(paths => (paths ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(HaveAFileName))
            .WithMessage("Remote file paths must have valid file names.");
        RuleFor(x => x)
            .Must(request => request.Files is not null &&
                request.Files.Count == request.GetRemoteFilePaths().Length)
            .WithMessage("Each file must have a corresponding remote file path.");
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
