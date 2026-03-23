namespace FpolyCafe.Application.Modules.Lookups.DTOs;

public record SizeLookupDto(int SizeId, string SizeName);
public record ToppingLookupDto(int ToppingId, string ToppingName, decimal Price, bool IsActive);
public record IngredientLookupDto(int IngredientId, string IngredientName, string Unit, decimal StockQuantity);
