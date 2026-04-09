using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Promotions.DTOs;
using FpolyCafe.Application.Modules.Promotions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _promotionService.GetAllAsync(cancellationToken));
    }

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PromotionDto>>> GetAvailable([FromQuery] decimal orderAmount, CancellationToken cancellationToken)
    {
        return Ok(await _promotionService.GetAvailableAsync(orderAmount, cancellationToken));
    }

    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<PromotionDto>> Validate(string code, [FromQuery] decimal orderAmount, CancellationToken cancellationToken)
    {
        return Ok(await _promotionService.ValidateAsync(code, orderAmount, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PromotionDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _promotionService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<PromotionDto>> GetByCode(string code, CancellationToken cancellationToken)
    {
        return Ok(await _promotionService.GetByCodeAsync(code, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePromotionDto request, CancellationToken cancellationToken)
    {
        var id = await _promotionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePromotionDto request, CancellationToken cancellationToken)
    {
        await _promotionService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _promotionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
