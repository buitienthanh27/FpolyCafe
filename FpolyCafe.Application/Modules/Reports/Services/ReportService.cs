using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Reports.Services;

public class ReportService : IReportService
{
    private readonly IAppDbContext _context;

    public ReportService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var todayRevenue = await _context.Bills
            .Where(b => b.CreatedAt >= today && b.Status == BillStatus.Finished)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        var todayBills = await _context.Bills
            .CountAsync(b => b.CreatedAt >= today && b.Status == BillStatus.Finished, cancellationToken);

        var totalProducts = await _context.Products.CountAsync(cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        return new DashboardSummaryDto(todayBills, todayRevenue, totalProducts, totalUsers);
    }

    public async Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var topProducts = await _context.BillDetails
            .Where(bd => bd.Bill.Status == BillStatus.Finished)
            .GroupBy(bd => new { bd.ProductId, bd.HistoricalProductName })
            .Select(g => new TopProductDto(
                g.Key.HistoricalProductName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.HistoricalPrice)))
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .ToListAsync(cancellationToken);

        return topProducts;
    }

    public async Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var report = await _context.Bills
            .Where(b => b.CreatedAt >= startDate && b.Status == BillStatus.Finished)
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenueDto(
                g.Key,
                g.Sum(x => x.TotalAmount),
                g.Count()))
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return report;
    }

    public async Task<AttendanceDashboardReportDto> GetAttendanceDashboardAsync(DateTime? date, CancellationToken cancellationToken = default)
    {
        var target = (date ?? DateTime.UtcNow).Date;
        var next = target.AddDays(1);
        var threshold = target.AddHours(8).AddMinutes(15);

        var attendances = await _context.Attendances
            .Where(x => x.CheckInTime >= target && x.CheckInTime < next)
            .ToListAsync(cancellationToken);

        return new AttendanceDashboardReportDto(
            target,
            attendances.Count(x => x.Status == AttendanceStatus.Working),
            attendances.Count(x => x.Status == AttendanceStatus.OnBreak),
            attendances.Count(x => x.Status == AttendanceStatus.Completed || x.Status == AttendanceStatus.Adjusted),
            attendances.Count(x => x.Status == AttendanceStatus.MissingCheckout),
            attendances.Count(x => x.CheckInTime > threshold),
            attendances.Sum(x => x.WorkedMinutes),
            attendances.Sum(x => x.OvertimeMinutes),
            attendances.Sum(x => x.SalaryAmount));
    }

    public async Task<IEnumerable<LateEmployeeDto>> GetLateEmployeesAsync(DateTime? date, int thresholdHour = 8, int thresholdMinute = 15, CancellationToken cancellationToken = default)
    {
        var target = (date ?? DateTime.UtcNow).Date;
        var next = target.AddDays(1);
        var threshold = target.AddHours(thresholdHour).AddMinutes(thresholdMinute);

        var items = await _context.Attendances
            .Include(x => x.Employee)
            .Where(x => x.CheckInTime >= target && x.CheckInTime < next && x.CheckInTime > threshold)
            .OrderByDescending(x => x.CheckInTime)
            .ToListAsync(cancellationToken);

        return items.Select(x => new LateEmployeeDto(
            x.EmployeeId,
            x.Employee.FullName,
            x.CheckInTime,
            Math.Max(0, (int)Math.Round((x.CheckInTime - threshold).TotalMinutes, MidpointRounding.AwayFromZero)),
            x.Status.ToString()));
    }

    public async Task<IEnumerable<OvertimeSummaryDto>> GetOvertimeSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var start = from?.Date ?? DateTime.UtcNow.Date.AddDays(-7);
        var endExclusive = (to?.Date ?? DateTime.UtcNow.Date).AddDays(1);

        return await _context.Attendances
            .Include(x => x.Employee)
            .Where(x => x.CheckInTime >= start && x.CheckInTime < endExclusive && x.OvertimeMinutes > 0)
            .GroupBy(x => new { x.EmployeeId, x.Employee.FullName })
            .Select(g => new OvertimeSummaryDto(
                g.Key.EmployeeId,
                g.Key.FullName,
                g.Count(),
                g.Sum(x => x.WorkedMinutes),
                g.Sum(x => x.OvertimeMinutes),
                g.Sum(x => x.SalaryAmount)))
            .OrderByDescending(x => x.TotalOvertimeMinutes)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MonthlyAttendanceSummaryDto>> GetMonthlyAttendanceSummaryAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .Include(x => x.Employee)
            .Where(x => x.CheckInTime.Month == month && x.CheckInTime.Year == year)
            .GroupBy(x => new { x.EmployeeId, x.Employee.FullName })
            .Select(g => new MonthlyAttendanceSummaryDto(
                g.Key.EmployeeId,
                g.Key.FullName,
                month,
                year,
                g.Count(),
                g.Sum(x => x.WorkedMinutes),
                g.Sum(x => x.OvertimeMinutes),
                g.Count(x => x.Status == AttendanceStatus.MissingCheckout),
                g.Sum(x => x.SalaryAmount)))
            .OrderByDescending(x => x.TotalSalaryAmount)
            .ToListAsync(cancellationToken);
    }
}
