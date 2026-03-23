using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Lookups.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Lookups.Services;

public class LookupService : ILookupService
{
    private readonly IAppDbContext _context;

    public LookupService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SizeLookupDto>> GetSizesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sizes
            .OrderBy(x => x.SizeId)
            .Select(x => new SizeLookupDto(x.SizeId, x.SizeName))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ToppingLookupDto>> GetToppingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Toppings
            .OrderBy(x => x.ToppingName)
            .Select(x => new ToppingLookupDto(x.ToppingId, x.ToppingName, x.Price, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<IngredientLookupDto>> GetIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients
            .OrderBy(x => x.IngredientName)
            .Select(x => new IngredientLookupDto(x.IngredientId, x.IngredientName, x.Unit, x.StockQuantity))
            .ToListAsync(cancellationToken);
    }
}
