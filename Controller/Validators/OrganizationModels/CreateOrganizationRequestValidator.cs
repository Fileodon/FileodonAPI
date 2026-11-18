using Controller.DTO.Organization;
using FluentValidation;

namespace Controller.Validators.OrganizationModels;

public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.OrgName)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.WorkspaceURL)
            .NotEmpty().WithMessage("Workspace URL is required.")
            .MaximumLength(100).WithMessage("Workspace URL must not exceed 100 characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact Email is required.")
            .EmailAddress().WithMessage("Contact Email must be a valid email address.");
    }
}
