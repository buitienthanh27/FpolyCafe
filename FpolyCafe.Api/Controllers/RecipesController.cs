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
public class RecipesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public RecipesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByProduct(int productId, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetRecipesByProductAsync(productId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateRecipeDto request, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.CreateRecipeAsync(request, cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _inventoryService.DeleteRecipeAsync(id, cancellationToken);
        return NoContent();
    }
}
