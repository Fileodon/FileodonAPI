namespace Controller.DTO.UserOrganization
{
    public record class CreateUserOrganizationRequest
    (
        int organization_id,
        string users_id,
        string role_id);
}
