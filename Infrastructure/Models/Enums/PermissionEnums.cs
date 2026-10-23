using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Models.Enums
{
    public class PermissionEnums
    {
        enum Permission
        {
            None = 0,
            Read = 1 << 0, // 1
            Update = 1 << 1, // 2
            Create = 1 << 2, // 4
            Delete = 1 << 3, // 8
            Execute = 1 << 4, // 16
        }
    }
}
