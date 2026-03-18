using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Modules.Categories.DTOs;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;

    public CategoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Select(c => new CategoryDto(c.CategoryId, c.Name, c.Description, c.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (category == null) throw new NotFoundException(nameof(Category), id);
        return new CategoryDto(category.CategoryId, category.Name, category.Description, category.IsActive);
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name, cancellationToken);
        if (exists)
            throw new BadRequestException("Danh mục đã tồn tại.");

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.CategoryId;
    }

    public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (category == null) throw new NotFoundException(nameof(Category), id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);
        if (category == null) throw new NotFoundException(nameof(Category), id);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
