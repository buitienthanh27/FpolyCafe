using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Topping
{
    public int ToppingId { get; set; }
    public string ToppingName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? IngredientId { get; set; }
    public decimal QuantityNeeded { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Ingredient? Ingredient { get; set; }
    public virtual ICollection<BillDetailTopping> BillDetailToppings { get; set; } = new List<BillDetailTopping>();
}
