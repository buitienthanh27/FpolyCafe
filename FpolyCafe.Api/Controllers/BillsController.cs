using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.POS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BillDto>>> GetRecentBills()
    {
        var result = await _billService.GetRecentBillsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BillDto>> GetBillById(int id)
    {
        var result = await _billService.GetBillByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateBill([FromBody] CreateBillRequestDto request)
    {
        var billId = await _billService.CreateBillAsync(request.UserId);
        return CreatedAtAction(nameof(GetBillById), new { id = billId }, billId);
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] AddBillItemRequestDto request)
    {
        await _billService.AddItemToBillAsync(id, request.ProductId, request.SizeId, request.ToppingIds, request.Quantity, request.Note ?? "");
        return Ok();
    }

    [HttpPut("items/{billDetailId}")]
    public async Task<IActionResult> UpdateItem(int billDetailId, [FromBody] UpdateBillItemRequestDto request)
    {
        await _billService.UpdateItemInBillAsync(billDetailId, request.Quantity, request.Note ?? "");
        return NoContent();
    }

    [HttpDelete("items/{billDetailId}")]
    public async Task<IActionResult> RemoveItem(int billDetailId)
    {
        await _billService.RemoveItemFromBillAsync(billDetailId);
        return NoContent();
    }

    [HttpPost("{id}/checkout")]
    public async Task<IActionResult> Checkout(int id)
    {
        await _billService.CheckoutBillAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _billService.CancelBillAsync(id);
        return NoContent();
    }
}
