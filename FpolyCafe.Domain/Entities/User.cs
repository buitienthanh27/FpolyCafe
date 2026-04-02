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

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public virtual ICollection<InventoryReceipt> InventoryReceipts { get; set; } = new List<InventoryReceipt>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<SalaryRule> SalaryRules { get; set; } = new List<SalaryRule>();
    public virtual ICollection<AttendanceAdjustment> AttendanceAdjustments { get; set; } = new List<AttendanceAdjustment>();
    public virtual ICollection<MonthlyPayroll> MonthlyPayrolls { get; set; } = new List<MonthlyPayroll>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
