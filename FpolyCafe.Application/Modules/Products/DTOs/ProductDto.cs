namespace FpolyCafe.Application.Modules.Products.DTOs;

public record ProductDto(int ProductId, string Name, int CategoryId, string CategoryName, decimal Price, string? ImageUrl, bool IsActive);
public record CreateProductDto(string Name, int CategoryId, decimal Price, string? ImageUrl);
public record UpdateProductDto(string Name, int CategoryId, decimal Price, string? ImageUrl, bool IsActive);
