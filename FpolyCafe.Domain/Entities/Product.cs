using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
