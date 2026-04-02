using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class BillDetail
{
    public int BillDetailId { get; set; }
    public int BillId { get; set; }
    public int ProductId { get; set; }
    public string HistoricalProductName { get; set; } = string.Empty;
    public decimal HistoricalPrice { get; set; }
    public int SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public virtual Bill Bill { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Size Size { get; set; } = null!;
    public virtual ICollection<BillDetailTopping> BillDetailToppings { get; set; } = new List<BillDetailTopping>();
}
