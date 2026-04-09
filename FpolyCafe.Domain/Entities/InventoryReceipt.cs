using System;
using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class InventoryReceipt
{
    public int ReceiptId { get; set; }
    public int UserId { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalCost { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<InventoryReceiptDetail> Details { get; set; } = new List<InventoryReceiptDetail>();
}
