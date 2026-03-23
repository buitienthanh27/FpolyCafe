using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Lookups.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("sizes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSizes()
    {
        return Ok(await _lookupService.GetSizesAsync());
    }

    [HttpGet("toppings")]
    [AllowAnonymous]
    public async Task<IActionResult> GetToppings()
    {
        return Ok(await _lookupService.GetToppingsAsync());
    }

    [HttpGet("ingredients")]
    [AllowAnonymous]
    public async Task<IActionResult> GetIngredients()
    {
        return Ok(await _lookupService.GetIngredientsAsync());
    }
}
