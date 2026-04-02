namespace FpolyCafe.Domain.Entities;

public class Recipe
{
    public int ProductId { get; set; }
    public int SizeId { get; set; }
    public int IngredientId { get; set; }
    public decimal QuantityNeeded { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Size Size { get; set; } = null!;
    public virtual Ingredient Ingredient { get; set; } = null!;
}
