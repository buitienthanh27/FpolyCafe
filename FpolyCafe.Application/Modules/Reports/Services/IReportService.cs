using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Modules.Reports.Services;

public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(int days = 7, CancellationToken cancellationToken = default);
    Task<AttendanceDashboardReportDto> GetAttendanceDashboardAsync(DateTime? date, CancellationToken cancellationToken = default);
    Task<IEnumerable<LateEmployeeDto>> GetLateEmployeesAsync(DateTime? date, int thresholdHour = 8, int thresholdMinute = 15, CancellationToken cancellationToken = default);
    Task<IEnumerable<OvertimeSummaryDto>> GetOvertimeSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IEnumerable<MonthlyAttendanceSummaryDto>> GetMonthlyAttendanceSummaryAsync(int month, int year, CancellationToken cancellationToken = default);
}

public record DashboardSummaryDto(int TotalOrders, decimal TotalRevenue, int TotalProducts, int TotalUsers);
public record TopProductDto(string ProductName, int QuantitySold, decimal Revenue);
public record DailyRevenueDto(DateTime Date, decimal Revenue, int OrderCount);
public record AttendanceDashboardReportDto(
    DateTime Date,
    int ActiveEmployees,
    int EmployeesOnBreak,
    int CompletedShifts,
    int MissingCheckoutShifts,
    int LateEmployees,
    int TotalWorkedMinutes,
    int TotalOvertimeMinutes,
    decimal TotalSalaryAmount);
public record LateEmployeeDto(int EmployeeId, string EmployeeName, DateTime CheckInTime, int LateMinutes, string Status);
public record OvertimeSummaryDto(int EmployeeId, string EmployeeName, int ShiftCount, int TotalWorkedMinutes, int TotalOvertimeMinutes, decimal TotalSalaryAmount);
public record MonthlyAttendanceSummaryDto(int EmployeeId, string EmployeeName, int Month, int Year, int ShiftCount, int WorkedMinutes, int OvertimeMinutes, int MissingCheckoutCount, decimal TotalSalaryAmount);
