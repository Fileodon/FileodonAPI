using Controller.DTO.UserOrganization;
using FluentValidation;

namespace Controller.Validators.UserOrganizationModels;

public class CreateUserOrganizationRequestValidator : AbstractValidator<CreateUserOrganizationRequest>
{
    public CreateUserOrganizationRequestValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithMessage("OrganizationId must be greater than 0.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.");
    }
}
