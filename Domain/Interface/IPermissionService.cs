using Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interface
{
    public interface IPermissionService : IBaseInterface<Permission>
    {
        //Task PostAsync(global::Controller.DTO.Permission.CreatePermissionRequest permission);
    }
}
