using System;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class SalaryRule
{
    public int SalaryRuleId { get; set; }
    public int? EmployeeId { get; set; }
    public RoleType? Role { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal NightShiftMultiplier { get; set; } = 1;
    public int MaxHoursPerShift { get; set; } = 12;
    public int StandardHoursPerShift { get; set; } = 8;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public bool IsActive { get; set; } = true;

    public virtual User? Employee { get; set; }
}
