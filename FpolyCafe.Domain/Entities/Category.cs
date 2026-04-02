using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
