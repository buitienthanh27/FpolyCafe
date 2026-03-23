using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Lookups.DTOs;

namespace FpolyCafe.Application.Modules.Lookups.Services;

public interface ILookupService
{
    Task<IEnumerable<SizeLookupDto>> GetSizesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ToppingLookupDto>> GetToppingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<IngredientLookupDto>> GetIngredientsAsync(CancellationToken cancellationToken = default);
}
