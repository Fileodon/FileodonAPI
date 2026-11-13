using Controller.DTO.Permission;
using Infrastructure.Models.Enums;

public static class PermissionUtils
{
    public static List<GetPermissionResponse> GetAllPermissions()
    {
        return  Enum.GetValues<PermissionFlags>()
            .Select(flag => new GetPermissionResponse(flag, (long)flag))
            .ToList();
    }

    public static List<GetPermissionResponse> GetPermissionsFromTotal(long total)
    {
        var permissions = (PermissionFlags)total;
        return Enum.GetValues<PermissionFlags>()
            .Where(flag => permissions.HasFlag(flag))
            .Select(flag => new GetPermissionResponse(flag, (long)flag))
            .ToList();
    }
}