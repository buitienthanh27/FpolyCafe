using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.AuditLogs.DTOs;
using FpolyCafe.Application.Modules.AuditLogs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> Get([FromQuery] string? action, [FromQuery] string? entityName, [FromQuery] int? userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _auditLogService.GetAuditLogsAsync(action, entityName, userId, from, to));
    }
}
