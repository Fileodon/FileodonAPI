
namespace Infrastructure.Models.Enums;

[Flags]
public enum PermissionFlags : long
{
    None = 0,
    Read = 1L << 0,
    Update = 1L << 1,
    Create = 1L << 2,
    Delete = 1L << 3,
    Execute = 1L << 4,

    All = Read | Update | Create | Delete | Execute
}
