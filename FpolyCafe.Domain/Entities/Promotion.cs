using System;
using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Promotion
{
    public int PromotionId { get; set; }
    public string? Name { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "Percentage";
    public decimal DiscountValue { get; set; } = 0;
    public decimal? MinBillAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
