using System;
using System.Collections.Generic;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class Bill
{
    public int BillId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Waiting;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
}
