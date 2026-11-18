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

    // Return 127.0.0.1 on localhost
    // https://stackoverflow.com/questions/72188657/how-to-get-client-ip-in-net-core-6-web-api
    [HttpGet("ip")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GetIp()
    {
        var ip = this.HttpContext.Connection.RemoteIpAddress;
        if (ip is null)
        {
            return BadRequest("Ip is not found");
        }

        if (ip.IsIPv4MappedToIPv6)
        {
            return Ok(ip.MapToIPv4().ToString());
        }

        return Ok(ip.ToString());
    }
}
