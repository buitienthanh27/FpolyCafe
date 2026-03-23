using System;
using System.Collections.Generic;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class MonthlyPayroll
{
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkedMinutes { get; set; }
    public int TotalOvertimeMinutes { get; set; }
    public decimal TotalNormalSalary { get; set; }
    public decimal TotalOvertimeSalary { get; set; }
    public decimal TotalSalary { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Generated;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public virtual User Employee { get; set; } = null!;
    public virtual ICollection<MonthlyPayrollDetail> Details { get; set; } = new List<MonthlyPayrollDetail>();
}
