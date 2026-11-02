using Domain.Interface;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Controller.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllRolesAsync()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleByIdAsync(string id)
    {
        var role = await _roleService.GetByIdAsync(id);

        if (role is null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoleAsync(
     [FromBody] RoleDTO request)
    {
        var role = new AspNetRole
        {
            Name = request.Name,
            OrganizationId = request.OrganizationId,
            PermissionMask = request.PermissionMask,
            NormalizedName = request.Name.ToUpper()
        };

        var createdRole = await _roleService.CreateAsync(role);

        return Ok(new
        {
            createdRole.Id,
            createdRole.Name,
            createdRole.OrganizationId,
            createdRole.PermissionMask,
            createdRole.NormalizedName
        });
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoleAsync(string id, [FromBody] RoleDTO request)
    {
        var role = new AspNetRole
        {
            Name = request.Name,
            OrganizationId = request.OrganizationId,
            PermissionMask = request.PermissionMask,
            NormalizedName = request.Name.ToUpper()
        };

        var updatedRole = await _roleService.UpdateAsync(id, role);

        if (updatedRole is null)
            return NotFound();

        return Ok(updatedRole);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoleAsync(string id)
    {
        var deleted = await _roleService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return Ok();
    }
}
