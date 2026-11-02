using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionController : ControllerBase
{
    [HttpGet]
    public IActionResult GetPermissions()
    {
        var permissions = Enum.GetValues(typeof(Permission))
            .Cast<Permission>()
            .Select(p => new
            {
                Id = p,
                Name = p.ToString()
            })
            .ToList();
        return Ok(permissions);
    }
    [HttpGet("{id}")]
    public IActionResult GetPermissionById(Permission id)
    {
        var permission = Enum.GetValues(typeof(Permission))
            .Cast<Permission>()
            .FirstOrDefault(p => p == id);
        if (permission == null)
        {
            return NotFound();
        }
        return Ok(new
        {
            Id = permission,
            Name = permission.ToString()
        });
    }
}
