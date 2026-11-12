using Controller.DTO.UserOrganization;
using FluentValidation;

namespace Controller.Validators.UserOrganizationModels;

public class EditUserOrganizationRequestValidator : AbstractValidator<EditUserOrganizationRequest>
{
    public EditUserOrganizationRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.");
    }
}
