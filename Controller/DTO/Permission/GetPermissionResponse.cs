namespace Controller.DTO.Permission;

public record class GetPermissionResponse
(
    string navn,
    long bit_value
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
