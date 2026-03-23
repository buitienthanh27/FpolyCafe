using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Modules.Attendance.DTOs;
using FpolyCafe.Application.Modules.Attendance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<AttendanceDto>> CheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _attendanceService.CheckInAsync(GetCurrentUserId(), request, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(result);
    }

    [HttpPost("break/start")]
    public async Task<ActionResult<AttendanceDto>> StartBreak([FromBody] StartBreakRequestDto request)
    {
        var result = await _attendanceService.StartBreakAsync(GetCurrentUserId(), request);
        return Ok(result);
    }

    [HttpPost("break/end")]
    public async Task<ActionResult<AttendanceDto>> EndBreak([FromBody] EndBreakRequestDto request)
    {
        var result = await _attendanceService.EndBreakAsync(GetCurrentUserId(), request);
        return Ok(result);
    }

    [HttpPost("check-out")]
    public async Task<ActionResult<AttendanceDto>> CheckOut([FromBody] CheckOutRequestDto request)
    {
        var result = await _attendanceService.CheckOutAsync(GetCurrentUserId(), request, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(result);
    }

    [HttpGet("me/today")]
    public async Task<ActionResult<AttendanceSummaryDto>> GetTodaySummary()
    {
        var result = await _attendanceService.GetTodaySummaryAsync(GetCurrentUserId());
        return Ok(result);
    }

    [HttpGet("me/open-shift")]
    public async Task<ActionResult<AttendanceDto?>> GetOpenShift()
    {
        var result = await _attendanceService.GetOpenShiftAsync(GetCurrentUserId());
        return Ok(result);
    }

    [HttpGet("me/history")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _attendanceService.GetAttendanceHistoryAsync(GetCurrentUserId(), from, to);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendances([FromQuery] int? employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
    {
        var result = await _attendanceService.GetAttendancesAsync(employeeId, from, to, status);
        return Ok(result);
    }

    [HttpPut("{attendanceId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AttendanceDto>> AdjustAttendance(int attendanceId, [FromBody] AdjustAttendanceRequestDto request)
    {
        var result = await _attendanceService.AdjustAttendanceAsync(attendanceId, GetCurrentUserId(), request, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(result);
    }

    [HttpPost("auto-close")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<object>> AutoClose([FromQuery] DateTime? cutoffTime)
    {
        var count = await _attendanceService.AutoCloseOpenShiftsAsync(cutoffTime, GetCurrentUserId(), HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new { updated = count });
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AttendanceDashboardDto>> GetDashboard([FromQuery] DateTime? date)
    {
        var result = await _attendanceService.GetDashboardAsync(date);
        return Ok(result);
    }

    [HttpGet("employee-summaries")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AttendanceEmployeeSummaryDto>>> GetEmployeeSummaries([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _attendanceService.GetEmployeeSummariesAsync(from, to);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedException("Không xác ð?nh ðý?c ngý?i dùng hi?n t?i.");
        }

        return parsedUserId;
    }
}
