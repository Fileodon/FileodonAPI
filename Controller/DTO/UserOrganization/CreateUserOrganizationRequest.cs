namespace Controller.DTO.UserOrganization
{
    public record class CreateUserOrganizationRequest
    (
        int organization_id,
        string email,
        string role_id
        );
}
