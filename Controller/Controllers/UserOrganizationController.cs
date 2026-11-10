using Controller.DTO.UserOrganization;
using Domain.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Controller.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserOrganizationController : ControllerBase
{
    private readonly IUserOrganizationService _userOrgService;

    public UserOrganizationController(IUserOrganizationService userOrgService)
    {
        _userOrgService = userOrgService;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(List<GetUserOrganizationResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserOrganizations()
    {
        string? currentUserId =
            User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var userOrganizations = await _userOrgService.GetByUserIdAsync(currentUserId);


        return Ok(userOrganizations.Select(org => org.ToGetUserOrganizationResponse()).ToList());
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetUserOrganizationResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetUserOrganizationById(int id)
    {
        var userOrganization = await _userOrgService.GetByIdAsync(id);
        if (userOrganization == null)
        {
            return NotFound();
        }
        return Ok(userOrganization.ToGetUserOrganizationResponse());
    }
    [HttpPost]
    [ProducesResponseType(202)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateUserOrganization(
     [FromBody] CreateUserOrganizationRequest request)
    {
        await _userOrgService.CreateUserOrganizationAsync(
            request.email,
            request.organization_id,
            request.role_id
        );
            
        return Accepted();
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> EditUserOrganization(
     [FromBody] EditUserOrganizationRequest request)
    {
        var updatedUserOrganization = await _userOrgService.EditUserOrganizationAsync(
            request.id,
            request.role_id
        );
            
        return Ok(updatedUserOrganization.ToGetUserOrganizationResponse());
    }




    [HttpDelete("{id:int}")]
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

    [HttpGet("organization/{organizationId:int}")]
    [ProducesResponseType(typeof(GetUserOrganizationResponse), 200)]
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
