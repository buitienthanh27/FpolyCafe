namespace FpolyCafe.Application.Modules.Categories.DTOs;

public record CategoryDto(int CategoryId, string Name, string? Description, bool IsActive);
public record CreateCategoryDto(string Name, string? Description);
public record UpdateCategoryDto(string Name, string? Description, bool IsActive);
