using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Inventory.DTOs;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Inventory.Services;

public class InventoryService : IInventoryService
{
    private readonly IAppDbContext _context;

    public InventoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IngredientDto>> GetAllIngredientsAsync(CancellationToken cancellationToken = default)
    {
        var ingredients = await _context.Ingredients.ToListAsync(cancellationToken);
        return ingredients.Select(i => {
            string status = "in_stock";
            if (i.StockQuantity <= 0) status = "out_of_stock";
            else if (i.StockQuantity <= i.MinimumStockLevel) status = "low_stock";
            
            return new IngredientDto(
                i.IngredientId, 
                i.IngredientName, 
                i.Unit, 
                i.StockQuantity, 
                i.MinimumStockLevel, 
                i.LastUnitCost,
                status,
                i.IsActive
            );
        });
    }

    public async Task<IngredientSummaryDto> GetIngredientSummaryAsync(CancellationToken cancellationToken = default)
    {
        var ingredients = await _context.Ingredients.ToListAsync(cancellationToken);
        return new IngredientSummaryDto(
            ingredients.Count,
            ingredients.Count(i => i.StockQuantity > 0 && i.StockQuantity <= i.MinimumStockLevel),
            ingredients.Count(i => i.StockQuantity <= 0),
            ingredients.Sum(i => i.StockQuantity * i.LastUnitCost)
        );
    }

    public async Task<int> CreateIngredientAsync(CreateIngredientDto request, CancellationToken cancellationToken = default)
    {
        var ingredient = new Ingredient
        {
            IngredientName = request.IngredientName,
            Unit = request.Unit,
            MinimumStockLevel = request.MinimumStockLevel,
            StockQuantity = request.StockQuantity,
            LastUnitCost = request.LastUnitCost,
            IsActive = true
        };
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync(cancellationToken);
        return ingredient.IngredientId;
    }

    public async Task<bool> UpdateIngredientAsync(int id, CreateIngredientDto request, CancellationToken cancellationToken = default)
    {
        var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.IngredientId == id, cancellationToken);
        if (ingredient == null) throw new NotFoundException("Ingredient", id);
        ingredient.IngredientName = request.IngredientName;
        ingredient.Unit = request.Unit;
        ingredient.MinimumStockLevel = request.MinimumStockLevel;
        ingredient.StockQuantity = request.StockQuantity;
        ingredient.LastUnitCost = request.LastUnitCost;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<RecipeDto>> GetRecipesByProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var recipes = await _context.Recipes
             .Include(r => r.Product)
             .Include(r => r.Size)
             .Include(r => r.Ingredient)
             .Where(r => r.ProductId == productId)
             .ToListAsync(cancellationToken);
             
        return recipes.Select(r => new RecipeDto(
            0, // no surrogate key - composite key
            r.ProductId,
            r.Product?.Name ?? "",
            r.SizeId,
            r.Size?.SizeName ?? "",
            r.IngredientId,
            r.Ingredient?.IngredientName ?? "",
            r.QuantityNeeded
        ));
    }

    public async Task<int> CreateRecipeAsync(CreateRecipeDto request, CancellationToken cancellationToken = default)
    {
        var recipe = new Recipe
        {
            ProductId = request.ProductId,
            SizeId = request.SizeId,
            IngredientId = request.IngredientId,
            QuantityNeeded = request.QuantityNeeded
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return recipe.ProductId; // no surrogate key, return ProductId
    }

    public async Task<bool> DeleteRecipeAsync(int id, CancellationToken cancellationToken = default)
    {
        // id here is treated as ProductId for simplicity
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.ProductId == id, cancellationToken);
        if (recipe == null) throw new NotFoundException("Recipe", id);
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CreateReceiptAsync(CreateInventoryReceiptDto request, int userId, CancellationToken cancellationToken = default)
    {
        var receipt = new InventoryReceipt
        {
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            Supplier = request.Supplier,
            Notes = request.Notes,
            TotalCost = 0
        };

        decimal total = 0;
        foreach (var d in request.Details)
        {
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.IngredientId == d.IngredientId, cancellationToken);
            if (ingredient == null) throw new NotFoundException("Ingredient", d.IngredientId);

            receipt.Details.Add(new InventoryReceiptDetail
            {
                IngredientId = d.IngredientId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            });

            ingredient.StockQuantity += d.Quantity;
            total += (d.Quantity * d.UnitPrice);
        }

        receipt.TotalCost = total;
        _context.InventoryReceipts.Add(receipt);
        await _context.SaveChangesAsync(cancellationToken);
        return receipt.ReceiptId;
    }

    public async Task<IEnumerable<InventoryReceiptDto>> GetAllReceiptsAsync(CancellationToken cancellationToken = default)
    {
        var receipts = await _context.InventoryReceipts
            .Include(r => r.Details)
                .ThenInclude(d => d.Ingredient)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return receipts.Select(r => new InventoryReceiptDto(
            r.ReceiptId,
            r.CreatedAt,
            r.Supplier,
            r.Notes,
            r.TotalCost,
            r.Details.Select(d => new InventoryReceiptDetailDto(
                0, // no surrogate key - composite key
                d.IngredientId,
                d.Ingredient?.IngredientName ?? "",
                d.Quantity,
                d.UnitPrice
            )).ToList()
        ));
    }
}
