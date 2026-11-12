using Controller.DTO.Permission;
using Domain.Interface;
using Infrastructure.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Controller.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(
        IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetPermissionResponse>), 200)]
    public async Task<IActionResult> GetPermissions()
    {
        List<GetPermissionResponse> permissions = Enum.GetValues<PermissionFlags>()
            .Select(flag => new GetPermissionResponse(flag.ToString(), (int)flag))
            .ToList();
        // var permissions =
        //     await _permissionService.GetAllAsync();

        return Ok(permissions);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        var permission = await _permissionService.GetByIdAsync(id);

        if (permission is null)
            return NotFound();

        return Ok(permission);
    }

    //I TVIVL OM VI SKAL HAVE POST OG PUT METODER HER, DA PERMISSIONER IKKE ÆNDRES. SKAL LAVES NÅR VI GÅR TIL AT LAVE RESURSE BASERET PERMISSIONS

    //[HttpPost]
    //public async Task<IActionResult> CreatePermission(
    //    [FromBody] DTO.Permission.CreatePermissionRequest permission)
    //{
    //    var created =
    //        await _permissionService.PostAsync(permission);

    //    return CreatedAtAction(
    //        nameof(GetPermissionById),
    //        new { id = created.Id },
    //        created);
    //}

    //[HttpPut("{id:int}")]
    //public async Task<IActionResult> UpdatePermission(
    //    int id,
    //    [FromBody] Permission permission)
    //{
    //    if (id != permission.Id)
    //        return BadRequest();

    //    var existing =
    //        await _permissionService.GetByIdAsync(id);

    //    if (existing is null)
    //        return NotFound();

    //    var updated =
    //        await _permissionService.UpdateAsync(permission);

    //    return Ok(updated);
    //}

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var deleted =
            await _permissionService.DeleteAsync(id);

        if (deleted is null)
            return NotFound();

        return NoContent();
    }
}