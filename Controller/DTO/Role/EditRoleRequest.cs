namespace Controller.DTO.Role;

public record class EditRoleRequest(
    string Id,
    string Name,
    int PermissionMask
);
