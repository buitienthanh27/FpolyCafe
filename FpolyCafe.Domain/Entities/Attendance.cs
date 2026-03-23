using System;
using System.Collections.Generic;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class Attendance
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int WorkedMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal SalaryAmount { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Working;
    public string? CheckInSource { get; set; }
    public string? CheckOutSource { get; set; }
    public string? CheckInIp { get; set; }
    public string? CheckOutIp { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Employee { get; set; } = null!;
    public virtual ICollection<AttendanceBreak> Breaks { get; set; } = new List<AttendanceBreak>();
    public virtual ICollection<AttendanceAdjustment> Adjustments { get; set; } = new List<AttendanceAdjustment>();
    public virtual ICollection<MonthlyPayrollDetail> PayrollDetails { get; set; } = new List<MonthlyPayrollDetail>();
}
