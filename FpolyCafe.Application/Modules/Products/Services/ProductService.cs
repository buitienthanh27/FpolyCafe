using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Modules.Products.DTOs;
using FpolyCafe.Application.Modules.Products.Services;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Products.Services;

public class ProductService : IProductService
{
    private readonly IAppDbContext _context;

    public ProductService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Select(p => new ProductDto(
                p.ProductId,
                p.Name,
                p.CategoryId,
                p.Category.Name,
                p.Price,
                p.ImageUrl,
                p.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        return new ProductDto(
            product.ProductId,
            product.Name,
            product.CategoryId,
            product.Category.Name,
            product.Price,
            product.ImageUrl,
            product.IsActive);
    }

    public async Task<int> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new BadRequestException("Category không tồn tại.");

        var product = new Product
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.ProductId;
    }

    public async Task<bool> UpdateProductAsync(int id, UpdateProductDto request, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new BadRequestException("Category không tồn tại.");

        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
