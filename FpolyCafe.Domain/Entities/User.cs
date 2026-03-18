using System.Collections.Generic;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public RoleType Role { get; set; } = RoleType.Staff;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public virtual ICollection<InventoryReceipt> InventoryReceipts { get; set; } = new List<InventoryReceipt>();
}
