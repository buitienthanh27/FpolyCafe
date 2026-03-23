using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Payroll.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<IEnumerable<MonthlyPayrollDto>>> Generate([FromBody] GeneratePayrollRequestDto request)
    {
        var result = await _payrollService.GenerateMonthlyPayrollAsync(request);
        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<MonthlyPayrollDto>>> GetMonthly([FromQuery] int month, [FromQuery] int year)
    {
        var result = await _payrollService.GetMonthlyPayrollsAsync(month, year);
        return Ok(result);
    }

    [HttpGet("{employeeId:int}/{year:int}/{month:int}")]
    public async Task<ActionResult<MonthlyPayrollDto>> GetEmployeePayroll(int employeeId, int year, int month)
    {
        var result = await _payrollService.GetEmployeePayrollAsync(employeeId, month, year);
        return Ok(result);
    }
}
