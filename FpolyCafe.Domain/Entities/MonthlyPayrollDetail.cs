namespace FpolyCafe.Domain.Entities;

public class MonthlyPayrollDetail
{
    public int PayrollDetailId { get; set; }
    public int PayrollId { get; set; }
    public int AttendanceId { get; set; }
    public int WorkedMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public decimal SalaryAmount { get; set; }

    public virtual MonthlyPayroll Payroll { get; set; } = null!;
    public virtual Attendance Attendance { get; set; } = null!;
}
