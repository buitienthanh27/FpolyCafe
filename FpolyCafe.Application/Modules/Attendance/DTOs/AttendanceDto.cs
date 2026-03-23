using System;
using System.Collections.Generic;

namespace FpolyCafe.Application.Modules.Attendance.DTOs;

public record CheckInRequestDto(string? Source, string? Notes);
public record StartBreakRequestDto(string? Note);
public record EndBreakRequestDto(string? Note);
public record CheckOutRequestDto(string? Source, string? Notes);
public record AdjustAttendanceRequestDto(DateTime CheckInTime, DateTime? CheckOutTime, string Reason, string? Notes);
public record AttendanceBreakDto(int BreakId, DateTime StartTime, DateTime? EndTime, int DurationMinutes, string Status, string? Note);
public record AttendanceDto(
    int AttendanceId,
    int EmployeeId,
    string EmployeeName,
    DateTime CheckInTime,
    DateTime? CheckOutTime,
    int WorkedMinutes,
    int BreakMinutes,
    int OvertimeMinutes,
    decimal SalaryAmount,
    string Status,
    string? Notes,
    IReadOnlyCollection<AttendanceBreakDto> Breaks);
public record AttendanceSummaryDto(
    AttendanceDto? CurrentShift,
    int TotalWorkedMinutesToday,
    int TotalOvertimeMinutesToday,
    int TotalCompletedShiftsToday);
public record AttendanceDashboardDto(
    DateTime Date,
    int ActiveEmployees,
    int EmployeesOnBreak,
    int CompletedShifts,
    int MissingCheckoutShifts,
    int TotalWorkedMinutes,
    int TotalOvertimeMinutes,
    decimal TotalSalaryAmount);
public record AttendanceEmployeeSummaryDto(int EmployeeId, string EmployeeName, int ShiftCount, int WorkedMinutes, int OvertimeMinutes, decimal SalaryAmount);
