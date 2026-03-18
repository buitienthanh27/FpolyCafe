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
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts([FromQuery] int count = 5)
    {
        var result = await _reportService.GetTopSellingProductsAsync(count);
        return Ok(result);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<IEnumerable<DailyRevenueDto>>> GetRevenueReport([FromQuery] int days = 7)
    {
        var result = await _reportService.GetRevenueReportAsync(days);
        return Ok(result);
    }
}
