using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Attendance.DTOs;
using FpolyCafe.Application.Modules.Attendance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public ShiftsController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAll([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        // For simplicity, we use date as from/to if provided
        DateTime? from = date?.Date;
        DateTime? to = date?.Date; // The service handles to.AddDays(1)
        
        return Ok(await _attendanceService.GetAttendancesAsync(null, from, to, status, cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AttendanceDashboardDto>> GetSummary([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetDashboardAsync(date, cancellationToken));
    }
}
