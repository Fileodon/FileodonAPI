using Controller.DTO.AuditLog;
using Controller.Middleware.Attribute;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
[RequireVerifiedEmail]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogController(IAuditLogService service)
    {
        _service = service;
    }

    [HttpGet("all/{organizationId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAuditLogs(int organizationId, [FromQuery] string? filter)
    {
        // Authorize - Måske det skulle authorizes pr resource, altså som man altid kan blive vist audit logs på de resourcer man har read på.
        // 

        var data = await _service.GetAllAuditLogsInOrganization(organizationId);

        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            data.organizationAuditLogs,
            data.SFTPConnectionAuditLogs,
            data.UserOrganizationAuditLogs,
            data.RoleAuditLogs,
            data.SFTPActionAuditLogs
        );
        return Ok(dto);
    }

    [HttpGet("organization/{organizationId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationAuditLogs(int organizationId)
    {
        var auditLogs = await _service.GetOrganizationAuditLogs(organizationId);
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            organizationAuditLogs: auditLogs
        );
        return Ok(dto);
    }
    [HttpGet("sftpConnection/{sftpConnectionId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSFTPConnectionAuditLogs(int sftpConnectionId)
    {
        var auditLogs = await _service.GetSFTPConnectionAuditLogs(sftpConnectionId);
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            sFTPConnectionAuditLogs: auditLogs
        );
        return Ok(dto);
    }
    [HttpGet("role/{roleId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleAuditLogs(string roleId)
    {
        var auditLogs = await _service.GetRoleAuditLogs(roleId);
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            roleAuditLogs: auditLogs
        );
        return Ok(dto);
    }

    [HttpGet("userOrganization/{userOrganizationId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserOrganizationAuditLogs(int userOrganizationId)
    {
        var auditLogs = await _service.GetUserOrganizationAuditLogs(userOrganizationId);
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            userOrganizationAuditLogs: auditLogs
        );
        return Ok(dto);
    }

    [HttpGet("sftpAction/{sftpConnectionId}")]
    [ProducesResponseType(typeof(GetAllAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSFTPActionAuditLogs(int sftpConnectionId)
    {
        var auditLogs = await _service.GetSFTPActionAuditLogs(sftpConnectionId);
        GetAllAuditLogsResponse dto = new GetAllAuditLogsResponse(
            sFTPActionAuditLogs: auditLogs
        );
        return Ok(dto);
    }
}