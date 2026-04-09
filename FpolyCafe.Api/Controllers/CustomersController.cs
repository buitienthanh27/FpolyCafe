using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Customers.DTOs;
using FpolyCafe.Application.Modules.Customers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetAllAsync(keyword, cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("phone/{phoneNumber}")]
    public async Task<ActionResult<CustomerDto>> GetByPhone(string phoneNumber, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetByPhoneAsync(phoneNumber, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateCustomerDto request, CancellationToken cancellationToken)
    {
        var id = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto request, CancellationToken cancellationToken)
    {
        await _customerService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
