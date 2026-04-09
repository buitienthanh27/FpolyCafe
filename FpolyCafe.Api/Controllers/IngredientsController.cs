using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Inventory.DTOs;
using FpolyCafe.Application.Modules.Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public IngredientsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IngredientDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetAllIngredientsAsync(cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IngredientSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetIngredientSummaryAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateIngredientDto request, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.CreateIngredientAsync(request, cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateIngredientDto request, CancellationToken cancellationToken)
    {
        await _inventoryService.UpdateIngredientAsync(id, request, cancellationToken);
        return NoContent();
    }
}
