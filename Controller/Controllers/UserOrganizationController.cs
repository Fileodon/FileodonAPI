using Controller.DTO;
using Controller.DTO.UserOrganization;
using Domain.Interface;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserOrganizationController: ControllerBase
{
    private readonly IUserOrganizationService _userOrganizationService;
    public UserOrganizationController(IUserOrganizationService userOrganizationService)
    {
        _userOrganizationService = userOrganizationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetUsersUserOrganizationRequest), 200)]
    public async Task<IActionResult> GetAllUserOrganizationsFromUser()
    {
        string? userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        IEnumerable<UserOrganization> userOrganizations = await _userOrganizationService.GetAllUserOrganizations(userId);

        var dto = new GetUsersUserOrganizationRequest(userOrganizations.ToList());
        return Ok(dto);
    }
}