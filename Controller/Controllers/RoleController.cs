using Controller.DTO.Role;
using Controller.Middleware.Attribute;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Controller.Controllers;

[Authorize]
[ApiController]
[Route("/organization/{organizationId}/[controller]")]
[RequireVerifiedEmail]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }
    [HttpGet]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.RoleRead)]
    [ProducesResponseType(typeof(List<GetRoleResponse>), 200)]
    [AllowUnverifiedEmail]
    public async Task<IActionResult> GetAllRolesAsync(int organizationId)
    {
        var roles = await _roleService.GetAllAsync(organizationId);
        return Ok(roles.Select(role => role.ToGetRoleResponse()));
    }
    [HttpGet("{id}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.RoleRead)]
    [ProducesResponseType(typeof(GetRoleResponse), 200)]
    [AllowUnverifiedEmail]
    public async Task<IActionResult> GetRoleByIdAsync(int organizationId, string id)
    {
        var role = await _roleService.GetByIdAsync(organizationId, id);

        if (role is null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.RoleCreate)]
    [ProducesResponseType(typeof(GetRoleResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> CreateRoleAsync(
        int organizationId,
     [FromBody] CreateRoleRequest request)
    {
        var role = new AspNetRole
        {
            Name = request.Name,
            OrganizationId = organizationId,
            IsSystemRole = false,
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
    [Permission(Infrastructure.Models.Enums.PermissionFlags.RoleUpdate)]
    [ProducesResponseType(typeof(GetRoleResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<IActionResult> UpdateRoleAsync(string id, int organizationId, [FromBody] EditRoleRequest request)
    {
        var role = new AspNetRole
        {
            Name = request.Name,
            OrganizationId = organizationId,
            PermissionMask = request.PermissionMask,
            NormalizedName = request.Name.ToUpper()
        };

        var updatedRole = await _roleService.UpdateAsync(organizationId, id, role);

        if (updatedRole is null)
            return NotFound();

        return Ok(updatedRole.ToGetRoleResponse());
    }
    [HttpDelete("{id}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.RoleDelete)]
    public async Task<IActionResult> DeleteRoleAsync(int organizationId, string id)
    {
        var deleted = await _roleService.DeleteAsync(organizationId, id);

        if (!deleted)
            return NotFound();

        return Ok();
    }
}
