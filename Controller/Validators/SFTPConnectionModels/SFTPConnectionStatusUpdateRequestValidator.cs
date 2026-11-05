using Controller.DTO.SFTPConnection;
using FluentValidation;

namespace Controller.Validators.SFTPConnectionModels;

public class SFTPConnectionStatusUpdateRequestValidator : AbstractValidator<SFTPConnectionStatusUpdateRequest>
{
    public SFTPConnectionStatusUpdateRequestValidator()
    {
        RuleFor(x => x.SFTPConnectionId)
            .GreaterThan(0)
            .WithMessage("SFTPConnectionId must be greater than 0.");
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("NewStatus must be a valid SFTP connection status.");
    }
}
