using System.Collections.Generic;
using System.Security.Claims;
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
public class InventoryReceiptsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryReceiptsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryReceiptDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetAllReceiptsAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateInventoryReceiptDto request, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        var userId = int.Parse(userIdStr);
        return Ok(await _inventoryService.CreateReceiptAsync(request, userId, cancellationToken));
    }
}
