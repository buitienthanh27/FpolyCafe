namespace FpolyCafe.Domain.Entities;

public class BillDetailTopping
{
    public int BillDetailId { get; set; }
    public int ToppingId { get; set; }
    public string HistoricalToppingName { get; set; } = string.Empty;
    public decimal HistoricalToppingPrice { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual BillDetail BillDetail { get; set; } = null!;
    public virtual Topping Topping { get; set; } = null!;
}
