using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.SalaryRules.DTOs;
using FpolyCafe.Application.Modules.SalaryRules.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class SalaryRulesController : ControllerBase
{
    private readonly ISalaryRuleService _salaryRuleService;

    public SalaryRulesController(ISalaryRuleService salaryRuleService)
    {
        _salaryRuleService = salaryRuleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalaryRuleDto>>> GetAll([FromQuery] int? employeeId, [FromQuery] string? role, [FromQuery] bool? isActive)
    {
        return Ok(await _salaryRuleService.GetSalaryRulesAsync(employeeId, role, isActive));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SalaryRuleDto>> GetById(int id)
    {
        return Ok(await _salaryRuleService.GetSalaryRuleByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SalaryRuleDto>> Create([FromBody] CreateSalaryRuleDto request)
    {
        var created = await _salaryRuleService.CreateSalaryRuleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.SalaryRuleId }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SalaryRuleDto>> Update(int id, [FromBody] UpdateSalaryRuleDto request)
    {
        return Ok(await _salaryRuleService.UpdateSalaryRuleAsync(id, request));
    }
}
