namespace FpolyCafe.Domain.Entities;

public class InventoryReceiptDetail
{
    public int ReceiptId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation properties
    public virtual InventoryReceipt InventoryReceipt { get; set; } = null!;
    public virtual Ingredient Ingredient { get; set; } = null!;
}
