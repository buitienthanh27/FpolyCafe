using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Products.DTOs;

namespace FpolyCafe.Application.Modules.Products.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(int id, UpdateProductDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
