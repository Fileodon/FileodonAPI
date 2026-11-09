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

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<GetUserOrganizationRequest>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUserOrganizations(string userId)
    {
        string? currentUserId =
            User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        var userOrganizationsQuery = await _userOrgService.WhereAsync(
            x => x.UserId == userId);

        var userOrganizations = await userOrganizationsQuery.ToListAsync();

        return Ok(userOrganizations.Select(org =>
      new GetUserOrganizationRequest(
          org.Id,
          org.OrganizationId,
          org.UserId,
          org.RoleId
      )).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserOrganizations()
    {
        var userOrganizations = await _userOrgService.GetAllAsync();
        return Ok(userOrganizations);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserOrganizationById(int id)
    {
        var userOrganization = await _userOrgService.GetByIdAsync(id);
        if (userOrganization == null)
        {
            return NotFound();
        }
        return Ok(userOrganization);
    }
    [HttpPost]
    public async Task<IActionResult> CreateUserOrganization(
     [FromBody] CreateUserOrganizationRequest request)
    {
        var createdUserOrganization =
            await _userOrgService.CreateUserOrganizationAsync(
                request.users_id,
                request.organization_id,
                request.role_id);

        return CreatedAtAction(
            nameof(GetUserOrganizationById),
            new { id = createdUserOrganization.Id },
            createdUserOrganization);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserOrganization(int id)
    {
        var deletedUserOrganization = await _userOrgService.DeleteAsync(id);
        if (deletedUserOrganization == null)
        {
            return NotFound();
        }
        return Ok(deletedUserOrganization);
    }

}
