using Infrastructure.Models.Enums;

namespace Controller.DTO.Permission;

public record class GetPermissionResponse
(
    PermissionFlags Name,
    long BitValue
);

// public static class GetPermissionsExtension
// {
//     public static GetPermissionResponse ToDTO(this Infrastructure.Models.Permission permissions)
//     {
//         return new GetPermissionResponse(
//           permissions.Id,
//           permissions.Navn,
//           permissions.BitValue
//         );
//     }
// }
