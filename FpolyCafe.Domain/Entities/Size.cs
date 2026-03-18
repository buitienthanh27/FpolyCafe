using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Size
{
    public int SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
}
