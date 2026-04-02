namespace FpolyCafe.Domain.Entities;

public class ProductSize
{
    public int ProductId { get; set; }
    public int SizeId { get; set; }
    public decimal PriceAdd { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Size Size { get; set; } = null!;
}
