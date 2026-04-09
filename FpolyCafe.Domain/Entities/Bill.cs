using System;
using System.Collections.Generic;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class Bill
{
    public int BillId { get; set; }
    public int UserId { get; set; }
    public int? CustomerId { get; set; }
    public int? PromotionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Waiting;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual Promotion? Promotion { get; set; }
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
}
