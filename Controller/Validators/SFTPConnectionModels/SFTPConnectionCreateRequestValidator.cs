using Controller.DTO.SFTPConnection;
using FluentValidation;

namespace Controller.Validators.SFTPConnectionModels;

public class SFTPConnectionCreateRequestValidator : AbstractValidator<SFTPConnectionCreateRequest>
{
    public SFTPConnectionCreateRequestValidator()
    {
        RuleFor(x => x.AuthCode).NotEmpty().Length(8).WithMessage("AuthCode must be 8 characters long.");
        // RuleFor(x => x.DisplayName).NotEmpty().Length(5, 100).WithMessage("DisplayName must be between 5 and 100 characters long.");
        RuleFor(x => x.Host).NotEmpty().Must(BeAValidHostOrIp).WithMessage("Host must be a valid hostname or IP address.");
        // RuleFor(x => x.Port).NotEmpty().InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
    }

    private bool BeAValidHostOrIp(string host)
    {
        return Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }
}
