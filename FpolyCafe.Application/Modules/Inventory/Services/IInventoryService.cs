using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Inventory.DTOs;

namespace FpolyCafe.Application.Modules.Inventory.Services;

public interface IInventoryService
{
    // Ingredients
    Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync(CancellationToken cancellationToken = default);
    Task<IngredientSummaryDto> GetIngredientSummaryAsync(CancellationToken cancellationToken = default);
    Task<int> CreateIngredientAsync(CreateIngredientDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateIngredientAsync(int id, CreateIngredientDto request, CancellationToken cancellationToken = default);
    
    // Recipes
    Task<IEnumerable<RecipeDto>> GetRecipesByProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<int> CreateRecipeAsync(CreateRecipeDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRecipeAsync(int id, CancellationToken cancellationToken = default);
    
    // Receipts
    Task<int> CreateReceiptAsync(CreateInventoryReceiptDto request, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryReceiptDto>> GetAllReceiptsAsync(CancellationToken cancellationToken = default);
}
