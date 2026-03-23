using System;
using System.Collections.Generic;

namespace FpolyCafe.Application.Modules.Payroll.Services;

public record PayrollDetailDto(int AttendanceId, DateTime CheckInTime, DateTime? CheckOutTime, int WorkedMinutes, int OvertimeMinutes, decimal SalaryAmount);
public record MonthlyPayrollDto(
    int PayrollId,
    int EmployeeId,
    string EmployeeName,
    int Month,
    int Year,
    int TotalWorkedMinutes,
    int TotalOvertimeMinutes,
    decimal TotalNormalSalary,
    decimal TotalOvertimeSalary,
    decimal TotalSalary,
    string Status,
    DateTime GeneratedAt,
    IReadOnlyCollection<PayrollDetailDto> Details);
public record GeneratePayrollRequestDto(int Month, int Year, int? EmployeeId);
