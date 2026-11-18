namespace Controller.DTO.UserOrganization
{
    public record class CreateUserOrganizationRequest
    (
        int OrganizationId,
        string Email,
        string RoleId
    );
}
