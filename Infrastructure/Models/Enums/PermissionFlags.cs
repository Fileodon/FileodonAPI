using System.Text.Json.Serialization;

namespace Infrastructure.Models.Enums;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PermissionFlags : long
{
    None = 0,
    OrganizationRead = 1L << 0,
    OrganizationUpdate = 1L << 1,
    OrganizationCreate = 1L << 2,
    OrganizationDelete = 1L << 3,
    OrganizationExecute = 1L << 4,
    SftpConnectionRead = 1L << 5,
    SftpConnectionUpdate = 1L << 6,
    SftpConnectionCreate = 1L << 7,
    SftpConnectionDelete = 1L << 8,
    SftpConnectionExecute = 1L << 9,
    UserOrganizationRead = 1L << 10,
    UserOrganizationUpdate = 1L << 11,
    UserOrganizationCreate = 1L << 12,
    UserOrganizationDelete = 1L << 13,
    UserOrganizationExecute = 1L << 14,
    RoleRead = 1L << 15,
    RoleUpdate = 1L << 16,
    RoleCreate = 1L << 17,
    RoleDelete = 1L << 18,
    RoleExecute = 1L << 19,
    SftpActionRead = 1L << 20, // Get the list of files, check if a file exists, etc.
    SftpActionUpdate = 1L << 21, // Update file contents, rename files, etc.
    SftpActionCreate = 1L << 22, // Upload files, create directories, etc.
    SftpActionDelete = 1L << 23, // Delete files, remove directories, etc.
    SftpctionExecute = 1L << 24, // Execute scripts or commands on the SFTP server.

    All = OrganizationRead | OrganizationUpdate | OrganizationCreate | OrganizationDelete | OrganizationExecute |
          SftpConnectionRead | SftpConnectionUpdate | SftpConnectionCreate | SftpConnectionDelete | SftpConnectionExecute |
          UserOrganizationRead | UserOrganizationUpdate | UserOrganizationCreate | UserOrganizationDelete | UserOrganizationExecute |
          RoleRead | RoleUpdate | RoleCreate | RoleDelete | RoleExecute | SftpActionRead | SftpActionUpdate | SftpActionCreate | SftpActionDelete | SftpctionExecute
}
