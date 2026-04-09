using System;
using System.Collections.Generic;

namespace FpolyCafe.Application.Modules.Inventory.DTOs;

public record IngredientDto(
    int IngredientId,
    string IngredientName,
    string Unit,
    decimal StockQuantity,
    decimal MinimumStockLevel,
    decimal LastUnitCost,
    string StockStatus,
    bool IsActive
);

public record CreateIngredientDto(
    string IngredientName,
    string Unit,
    decimal StockQuantity,
    decimal MinimumStockLevel,
    decimal LastUnitCost,
    bool IsActive = true
);

public record IngredientSummaryDto(
    int TotalIngredients,
    int LowStockIngredients,
    int OutOfStockIngredients,
    decimal TotalInventoryValue
);

public record RecipeDto(
    int RecipeId,
    int ProductId,
    string ProductName,
    int SizeId,
    string SizeName,
    int IngredientId,
    string IngredientName,
    decimal QuantityNeeded
);

public record CreateRecipeDto(
    int ProductId,
    int SizeId,
    int IngredientId,
    decimal QuantityNeeded
);

public record InventoryReceiptDto(
    int ReceiptId,
    DateTime ReceivedDate,
    string Supplier,
    string Notes,
    decimal TotalAmount,
    List<InventoryReceiptDetailDto> Details
);

public record InventoryReceiptDetailDto(
    int DetailId,
    int IngredientId,
    string IngredientName,
    decimal Quantity,
    decimal UnitPrice
);

public record CreateInventoryReceiptDto(
    string Supplier,
    string Notes,
    List<CreateInventoryReceiptDetailDto> Details
);

public record CreateInventoryReceiptDetailDto(
    int IngredientId,
    decimal Quantity,
    decimal UnitPrice
);
