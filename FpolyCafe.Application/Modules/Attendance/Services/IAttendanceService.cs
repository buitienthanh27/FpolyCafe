using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Attendance.DTOs;

namespace FpolyCafe.Application.Modules.Attendance.Services;

public interface IAttendanceService
{
    Task<AttendanceDto> CheckInAsync(int employeeId, CheckInRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AttendanceDto> StartBreakAsync(int employeeId, StartBreakRequestDto request, CancellationToken cancellationToken = default);
    Task<AttendanceDto> EndBreakAsync(int employeeId, EndBreakRequestDto request, CancellationToken cancellationToken = default);
    Task<AttendanceDto> CheckOutAsync(int employeeId, CheckOutRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AttendanceSummaryDto> GetTodaySummaryAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<AttendanceDto?> GetOpenShiftAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceDto>> GetAttendanceHistoryAsync(int employeeId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(int? employeeId, DateTime? from, DateTime? to, string? status, CancellationToken cancellationToken = default);
    Task<AttendanceDto> AdjustAttendanceAsync(int attendanceId, int adjustedByUserId, AdjustAttendanceRequestDto request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<int> AutoCloseOpenShiftsAsync(DateTime? cutoffTime, int? performedByUserId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AttendanceDashboardDto> GetDashboardAsync(DateTime? date, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttendanceEmployeeSummaryDto>> GetEmployeeSummariesAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
