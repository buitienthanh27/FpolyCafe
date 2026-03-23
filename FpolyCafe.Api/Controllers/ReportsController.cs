using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Reports.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    [HttpGet("top-products")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts([FromQuery] int count = 5)
    {
        var result = await _reportService.GetTopSellingProductsAsync(count);
        return Ok(result);
    }

    [HttpGet("revenue")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<DailyRevenueDto>>> GetRevenueReport([FromQuery] int days = 7)
    {
        var result = await _reportService.GetRevenueReportAsync(days);
        return Ok(result);
    }

    [HttpGet("attendance-dashboard")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AttendanceDashboardReportDto>> GetAttendanceDashboard([FromQuery] DateTime? date)
    {
        var result = await _reportService.GetAttendanceDashboardAsync(date);
        return Ok(result);
    }

    [HttpGet("late-employees")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<LateEmployeeDto>>> GetLateEmployees([FromQuery] DateTime? date, [FromQuery] int thresholdHour = 8, [FromQuery] int thresholdMinute = 15)
    {
        var result = await _reportService.GetLateEmployeesAsync(date, thresholdHour, thresholdMinute);
        return Ok(result);
    }

    [HttpGet("overtime-summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<OvertimeSummaryDto>>> GetOvertimeSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _reportService.GetOvertimeSummaryAsync(from, to);
        return Ok(result);
    }

    [HttpGet("monthly-attendance-summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<MonthlyAttendanceSummaryDto>>> GetMonthlyAttendanceSummary([FromQuery] int month, [FromQuery] int year)
    {
        var result = await _reportService.GetMonthlyAttendanceSummaryAsync(month, year);
        return Ok(result);
    }
}
