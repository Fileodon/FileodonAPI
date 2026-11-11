namespace Controller.DTO.UserOrganization;

public record class GetUserOrganizationRequest(
    int id,
    int? organization_id,
    string? users_id,
    string role_id);
