using System.Collections.Generic;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Products.DTOs;
using FpolyCafe.Application.Modules.Products.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var result = await _productService.GetProductsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateProduct([FromBody] CreateProductDto request)
    {
        var productId = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProductById), new { id = productId }, productId);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto request)
    {
        var result = await _productService.UpdateProductAsync(id, request);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result ? NoContent() : NotFound();
    }
}
