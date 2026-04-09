using System;
using System.Collections.Generic;

namespace FpolyCafe.Domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RewardPoints { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
