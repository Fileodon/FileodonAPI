using Controller.DTO.SFTPConnection;
using FluentValidation;

namespace Controller.Validators.SFTPConnectionModels;

public class SFTPConnectionCreateRequestValidator : AbstractValidator<SFTPConnectionCreateRequest>
{
    public SFTPConnectionCreateRequestValidator()
    {
        RuleFor(x => x.AuthCode).NotEmpty().Length(8).WithMessage("AuthCode must be 8 characters long.");
        RuleFor(x => x.DisplayName).Custom((displayName, context) =>
        {
            if (displayName is not null && (displayName.Length < 5 || displayName.Length > 100))
            {
                context.AddFailure("DisplayName must be between 5 and 100 characters long.");
            }
        });
        RuleFor(x => x.Host).NotEmpty().Must(BeAValidHostOrIp).WithMessage("Host must be a valid hostname or IP address.");
        RuleFor(x => x.Port).Custom((port, context) =>
        {
            if (port is not null && (port < 1 || port > 65535))
            {
                context.AddFailure("Port must be between 1 and 65535.");
            }
        });
        RuleFor(x => x.IntermediaryPort)
            .InclusiveBetween(1, 65535)
            .WithMessage("IntermediaryPort must be between 1 and 65535.");
    }

    private bool BeAValidHostOrIp(string host)
    {
        return Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }
}
