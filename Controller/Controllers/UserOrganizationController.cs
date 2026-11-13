using Controller.DTO.UserOrganization;
using Controller.Middleware.Attribute;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Controller.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class UserOrganizationController : ControllerBase
{
    private readonly IUserOrganizationService _userOrgService;

    public UserOrganizationController(IUserOrganizationService userOrgService)
    {
        _userOrgService = userOrgService;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(List<GetUsersUserOrganizationResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserOrganizations()
    {
        string? currentUserId =
            User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var userOrganizations = await _userOrgService.GetByUserIdAsync(currentUserId);

        return Ok(userOrganizations.Select(org => org.ToGetUsersUserOrganizationResponse()).ToList());
    }

    [HttpGet("/organization/{organizationId}/[controller]/{id:int}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationRead)]
    [ProducesResponseType(typeof(GetUserOrganizationResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserOrganizationById(int id)
    {
        var userOrganization = await _userOrgService.GetByIdAsync(id);
        if (userOrganization == null)
        {
            return NotFound();
        }
        return Ok(userOrganization.ToGetUserOrganizationResponse());
    }
    [HttpPost("/organization/{organizationId:int}/[controller]")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationCreate)]
    [ProducesResponseType(202)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateUserOrganization(
        int organizationId,
        [FromBody] CreateUserOrganizationRequest request)
    {
        await _userOrgService.CreateUserOrganizationAsync(
            request.Email,
            request.OrganizationId,
            request.RoleId
        );

        return Accepted();
    }

    [HttpPut("/organization/{organizationId:int}/[controller]/{id:int}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationUpdate)]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> EditUserOrganization(
        int organizationId,
     [FromBody] EditUserOrganizationRequest request)
    {
        var updatedUserOrganization = await _userOrgService.EditUserOrganizationAsync(
            request.Id,
            organizationId,
            request.RoleId
        );

        return Ok(updatedUserOrganization.ToGetUserOrganizationResponse());
    }



    [HttpDelete("/organization/{organizationId:int}/[controller]/{id:int}")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationDelete)]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteUserOrganization(int id)
    {
        var deletedUserOrganization = await _userOrgService.DeleteAsync(id);
        if (deletedUserOrganization == null)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpGet("/organization/{organizationId:int}/[controller]")]
    [Permission(Infrastructure.Models.Enums.PermissionFlags.UserOrganizationRead)]
    [ProducesResponseType(typeof(List<GetUserOrganizationResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetOrganizationUserOrganizations(int organizationId)
    {
        // TODO check permissions of user on organization
        var userOrganizations = await _userOrgService.GetByOrganizationIdAsync(organizationId);
        if (userOrganizations == null)
        {
            return NotFound();
        }
        return Ok(userOrganizations.Select(org => org.ToGetUserOrganizationResponse()).ToList());
    }

}
