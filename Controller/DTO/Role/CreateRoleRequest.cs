namespace Controller.DTO.Role;

public record class CreateRoleRequest(
    string Name,
    int PermissionMask
);
