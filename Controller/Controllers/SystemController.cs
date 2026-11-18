using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult HealthCheck()
    {
        return Ok("System is healthy");
    }
}
