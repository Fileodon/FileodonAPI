using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Models.DTO
{
    public class RoleDTO
    {
        public string Name { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public int PermissionMask { get; set; }
    }
}