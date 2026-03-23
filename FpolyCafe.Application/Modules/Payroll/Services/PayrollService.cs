using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Payroll.Services;

public class PayrollService : IPayrollService
{
    private readonly IAppDbContext _context;

    public PayrollService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonthlyPayrollDto>> GenerateMonthlyPayrollAsync(GeneratePayrollRequestDto request, CancellationToken cancellationToken = default)
    {
        var attendancesQuery = _context.Attendances
            .Include(x => x.Employee)
            .Where(x => x.CheckInTime.Year == request.Year
                && x.CheckInTime.Month == request.Month
                && (x.Status == AttendanceStatus.Completed || x.Status == AttendanceStatus.Adjusted || x.Status == AttendanceStatus.MissingCheckout));

        if (request.EmployeeId.HasValue)
        {
            attendancesQuery = attendancesQuery.Where(x => x.EmployeeId == request.EmployeeId.Value);
        }

        var attendances = await attendancesQuery.OrderBy(x => x.CheckInTime).ToListAsync(cancellationToken);
        var payrolls = new List<MonthlyPayroll>();

        foreach (var group in attendances.GroupBy(x => new { x.EmployeeId, x.Employee.FullName }))
        {
            var existing = await _context.MonthlyPayrolls
                .Include(x => x.Details)
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(
                    x => x.EmployeeId == group.Key.EmployeeId && x.Month == request.Month && x.Year == request.Year,
                    cancellationToken);

            if (existing == null)
            {
                existing = new MonthlyPayroll
                {
                    EmployeeId = group.Key.EmployeeId,
                    Month = request.Month,
                    Year = request.Year,
                    GeneratedAt = DateTime.UtcNow,
                    Status = PayrollStatus.Generated
                };
                _context.MonthlyPayrolls.Add(existing);
            }
            else if (existing.Details.Any())
            {
                _context.MonthlyPayrollDetails.RemoveRange(existing.Details);
            }

            var normalSalary = 0m;
            var overtimeSalary = 0m;
            var details = new List<MonthlyPayrollDetail>();
            foreach (var attendance in group)
            {
                var payrollDetail = new MonthlyPayrollDetail
                {
                    AttendanceId = attendance.AttendanceId,
                    WorkedMinutes = attendance.WorkedMinutes,
                    OvertimeMinutes = attendance.OvertimeMinutes,
                    SalaryAmount = attendance.SalaryAmount
                };
                details.Add(payrollDetail);

                var normalMinutes = Math.Max(0, attendance.WorkedMinutes - attendance.OvertimeMinutes);
                if (attendance.WorkedMinutes > 0)
                {
                    normalSalary += attendance.SalaryAmount * normalMinutes / attendance.WorkedMinutes;
                    overtimeSalary += attendance.SalaryAmount * attendance.OvertimeMinutes / attendance.WorkedMinutes;
                }
            }

            existing.TotalWorkedMinutes = group.Sum(x => x.WorkedMinutes);
            existing.TotalOvertimeMinutes = group.Sum(x => x.OvertimeMinutes);
            existing.TotalNormalSalary = Math.Round(normalSalary, 2, MidpointRounding.AwayFromZero);
            existing.TotalOvertimeSalary = Math.Round(overtimeSalary, 2, MidpointRounding.AwayFromZero);
            existing.TotalSalary = group.Sum(x => x.SalaryAmount);
            existing.GeneratedAt = DateTime.UtcNow;
            existing.Status = PayrollStatus.Generated;
            existing.Details = details;
            payrolls.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return payrolls.Select(MapPayroll).ToList();
    }

    public async Task<IEnumerable<MonthlyPayrollDto>> GetMonthlyPayrollsAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        var payrolls = await _context.MonthlyPayrolls
            .Include(x => x.Employee)
            .Include(x => x.Details)
            .ThenInclude(x => x.Attendance)
            .Where(x => x.Month == month && x.Year == year)
            .OrderByDescending(x => x.TotalSalary)
            .ToListAsync(cancellationToken);

        return payrolls.Select(MapPayroll);
    }

    public async Task<MonthlyPayrollDto> GetEmployeePayrollAsync(int employeeId, int month, int year, CancellationToken cancellationToken = default)
    {
        var payroll = await _context.MonthlyPayrolls
            .Include(x => x.Employee)
            .Include(x => x.Details)
            .ThenInclude(x => x.Attendance)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Month == month && x.Year == year, cancellationToken);

        if (payroll == null)
        {
            throw new NotFoundException("MonthlyPayroll", $"{employeeId}-{month}-{year}");
        }

        return MapPayroll(payroll);
    }

    private static MonthlyPayrollDto MapPayroll(MonthlyPayroll payroll)
    {
        return new MonthlyPayrollDto(
            payroll.PayrollId,
            payroll.EmployeeId,
            payroll.Employee?.FullName ?? string.Empty,
            payroll.Month,
            payroll.Year,
            payroll.TotalWorkedMinutes,
            payroll.TotalOvertimeMinutes,
            payroll.TotalNormalSalary,
            payroll.TotalOvertimeSalary,
            payroll.TotalSalary,
            payroll.Status.ToString(),
            payroll.GeneratedAt,
            payroll.Details
                .OrderBy(x => x.Attendance?.CheckInTime)
                .Select(x => new PayrollDetailDto(
                    x.AttendanceId,
                    x.Attendance?.CheckInTime ?? DateTime.MinValue,
                    x.Attendance?.CheckOutTime,
                    x.WorkedMinutes,
                    x.OvertimeMinutes,
                    x.SalaryAmount))
                .ToList());
    }
}
