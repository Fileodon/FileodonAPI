using Controller.DTO.SFTPAction;
using FluentValidation;

namespace Controller.Validators.SFTPActionModels;

public class SFTPActionDistributeRequestValidator : AbstractValidator<SFTPActionDistributeRequest>
{
    public SFTPActionDistributeRequestValidator()
    {
        RuleFor(x => x.Locations)
            .NotEmpty().WithMessage("At least one destination must be specified.")
            .ForEach(locationRule => locationRule.SetValidator(new SFTPActionDistributionLocationValidator()))
            .Must(locations => locations.Select(location => location.SftpConnectionId).Distinct().Count() == locations.Count())
                .WithMessage("Each SFTP connection may only be selected once.");
        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("At least one file must be uploaded.")
            .ForEach(fileRule => fileRule.Must(file => !string.IsNullOrWhiteSpace(file.FileName))
                .WithMessage("Every uploaded file must have a file name."));
    }
}

public class SFTPActionDistributionLocationValidator : AbstractValidator<SFTPActionDistributeLocation>
{
    public SFTPActionDistributionLocationValidator()
    {
        RuleFor(x => x.SftpConnectionId)
            .GreaterThan(0).WithMessage("SFTP connection ID must be greater than 0.");
        RuleFor(x => x.RemotePath)
            .NotEmpty().WithMessage("Remote path must not be empty.");
    }
}
