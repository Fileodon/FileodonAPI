namespace Controller.DTO.UserOrganization;

public record class GetUserOrganizationRequest(
    int Id,
    int OrganizationId,
    string UsersId,
    string RoleId);
