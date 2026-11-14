using Controller.DTO.Permission;

namespace Controller.DTO.Role;

public record class GetRoleResponse
(
    string Id,
    string Name,
    bool IsSystemRole,
    long PermissionMask,
    List<GetPermissionResponse> Permissions
);


public static class GetRoleResponseExtensions
{
    public static GetRoleResponse ToGetRoleResponse(this Infrastructure.Models.AspNetRole role) =>
        new(
            Id: role.Id,
            Name: role.Name,
            IsSystemRole: role.IsSystemRole,
            PermissionMask: role.PermissionMask,
            Permissions: PermissionUtils.GetPermissionsFromTotal(role.PermissionMask)
        );
}