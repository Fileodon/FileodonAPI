using Controller.DTO.Permission;

namespace Controller.DTO.Organization;

public record GetOrganizationResponse(
    int Id,
    string OrgName,
    string WorkspaceURL,
    string ContactEmail,
    List<GetPermissionResponse> Permissions
);

public static class GetOrganizationRequestExtensions
{
    public static GetOrganizationResponse ToGetOrganizationResponse(this Infrastructure.Models.Organization organization) =>
        new(
            organization.Id,
            organization.OrgName,
            organization.WorkspaceURL,
            organization.ContactEmail,
            PermissionUtils.GetPermissionsFromTotal(organization.UserOrganizations.First().Role.PermissionMask)
        );
}