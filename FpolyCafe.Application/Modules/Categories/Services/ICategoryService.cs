using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Categories.DTOs;

namespace FpolyCafe.Application.Modules.Categories.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateCategoryAsync(CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
