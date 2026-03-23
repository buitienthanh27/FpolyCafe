using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Attendance.DTOs;
using DomainAttendance = FpolyCafe.Domain.Entities.Attendance;
using DomainAttendanceBreak = FpolyCafe.Domain.Entities.AttendanceBreak;
using DomainAttendanceAdjustment = FpolyCafe.Domain.Entities.AttendanceAdjustment;
using DomainAuditLog = FpolyCafe.Domain.Entities.AuditLog;
using DomainSalaryRule = FpolyCafe.Domain.Entities.SalaryRule;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Attendance.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAppDbContext _context;

    public AttendanceService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto> CheckInAsync(int employeeId, CheckInRequestDto request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExists(employeeId, cancellationToken);
        await EnsureNoOpenShift(employeeId, cancellationToken);

        var attendance = new DomainAttendance
        {
            EmployeeId = employeeId,
            CheckInTime = DateTime.UtcNow,
            Status = AttendanceStatus.Working,
            CheckInSource = request.Source,
            CheckInIp = ipAddress,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditLogAsync(employeeId, "Attendance.CheckIn", nameof(DomainAttendance), attendance.AttendanceId.ToString(), null, attendance, ipAddress, cancellationToken);

        return await GetAttendanceDtoById(attendance.AttendanceId, cancellationToken);
    }

    public async Task<AttendanceDto> StartBreakAsync(int employeeId, StartBreakRequestDto request, CancellationToken cancellationToken = default)
    {
        var attendance = await GetOpenAttendanceEntity(employeeId, cancellationToken);
        if (attendance.Status == AttendanceStatus.OnBreak)
        {
            throw new BadRequestException("Ca l�m hi?n �ang trong tr?ng th�i ngh?.");
        }

        var breakEntity = new DomainAttendanceBreak
        {
            AttendanceId = attendance.AttendanceId,
            StartTime = DateTime.UtcNow,
            Status = BreakStatus.Active,
            Note = request.Note
        };

        _context.AttendanceBreaks.Add(breakEntity);
        attendance.Status = AttendanceStatus.OnBreak;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetAttendanceDtoById(attendance.AttendanceId, cancellationToken);
    }

    public async Task<AttendanceDto> EndBreakAsync(int employeeId, EndBreakRequestDto request, CancellationToken cancellationToken = default)
    {
        var attendance = await GetOpenAttendanceEntity(employeeId, cancellationToken);
        var activeBreak = attendance.Breaks.OrderByDescending(x => x.StartTime).FirstOrDefault(x => x.Status == BreakStatus.Active);
        if (activeBreak == null)
        {
            throw new BadRequestException("Kh�ng c� phi�n ngh? �ang m?.");
        }

        var endTime = DateTime.UtcNow;
        activeBreak.EndTime = endTime;
        activeBreak.DurationMinutes = GetPositiveMinutes(activeBreak.StartTime, endTime);
        activeBreak.Status = BreakStatus.Completed;
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            activeBreak.Note = request.Note;
        }

        attendance.Status = AttendanceStatus.Working;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetAttendanceDtoById(attendance.AttendanceId, cancellationToken);
    }

    public async Task<AttendanceDto> CheckOutAsync(int employeeId, CheckOutRequestDto request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var attendance = await GetOpenAttendanceEntity(employeeId, cancellationToken);
        var now = DateTime.UtcNow;

        CloseActiveBreaks(attendance, now);
        await ApplyAttendanceCalculationAsync(attendance, now, attendance.Status == AttendanceStatus.MissingCheckout, request.Notes, cancellationToken);
        attendance.CheckOutSource = request.Source;
        attendance.CheckOutIp = ipAddress;
        attendance.Status = AttendanceStatus.Completed;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditLogAsync(employeeId, "Attendance.CheckOut", nameof(DomainAttendance), attendance.AttendanceId.ToString(), null, attendance, ipAddress, cancellationToken);

        return await GetAttendanceDtoById(attendance.AttendanceId, cancellationToken);
    }

    public async Task<AttendanceSummaryDto> GetTodaySummaryAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);

        var attendances = await _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .Where(x => x.EmployeeId == employeeId && x.CheckInTime >= from && x.CheckInTime < to)
            .OrderByDescending(x => x.CheckInTime)
            .ToListAsync(cancellationToken);

        return new AttendanceSummaryDto(
            MapAttendance(attendances.FirstOrDefault(x => x.CheckOutTime == null || x.Status == AttendanceStatus.Working || x.Status == AttendanceStatus.OnBreak)),
            attendances.Sum(x => x.WorkedMinutes),
            attendances.Sum(x => x.OvertimeMinutes),
            attendances.Count(x => x.Status == AttendanceStatus.Completed || x.Status == AttendanceStatus.Adjusted || x.Status == AttendanceStatus.MissingCheckout));
    }

    public async Task<AttendanceDto?> GetOpenShiftAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var attendance = await _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .Where(x => x.EmployeeId == employeeId && (x.Status == AttendanceStatus.Working || x.Status == AttendanceStatus.OnBreak))
            .OrderByDescending(x => x.CheckInTime)
            .FirstOrDefaultAsync(cancellationToken);

        return MapAttendance(attendance);
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceHistoryAsync(int employeeId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .Where(x => x.EmployeeId == employeeId)
            .AsQueryable();

        query = ApplyAttendanceFilters(query, from, to, null);
        var items = await query.OrderByDescending(x => x.CheckInTime).ToListAsync(cancellationToken);
        return items.Select(MapAttendance).Where(x => x != null).Select(x => x!);
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(int? employeeId, DateTime? from, DateTime? to, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        query = ApplyAttendanceFilters(query, from, to, status);
        var items = await query.OrderByDescending(x => x.CheckInTime).ToListAsync(cancellationToken);
        return items.Select(MapAttendance).Where(x => x != null).Select(x => x!);
    }

    public async Task<AttendanceDto> AdjustAttendanceAsync(int attendanceId, int adjustedByUserId, AdjustAttendanceRequestDto request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var attendance = await _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .FirstOrDefaultAsync(x => x.AttendanceId == attendanceId, cancellationToken);

        if (attendance == null)
        {
            throw new NotFoundException(nameof(DomainAttendance), attendanceId);
        }

        await EnsureEmployeeExists(adjustedByUserId, cancellationToken);

        var oldSnapshot = new
        {
            attendance.CheckInTime,
            attendance.CheckOutTime,
            attendance.WorkedMinutes,
            attendance.BreakMinutes,
            attendance.OvertimeMinutes,
            attendance.SalaryAmount,
            attendance.Status,
            attendance.Notes
        };

        var adjustment = new DomainAttendanceAdjustment
        {
            AttendanceId = attendance.AttendanceId,
            AdjustedByUserId = adjustedByUserId,
            Reason = request.Reason,
            OldCheckInTime = attendance.CheckInTime,
            OldCheckOutTime = attendance.CheckOutTime,
            OldWorkedMinutes = attendance.WorkedMinutes,
            NewCheckInTime = request.CheckInTime,
            NewCheckOutTime = request.CheckOutTime
        };

        attendance.CheckInTime = request.CheckInTime;
        attendance.CheckOutTime = request.CheckOutTime;
        attendance.Notes = request.Notes;
        attendance.UpdatedAt = DateTime.UtcNow;

        var effectiveCheckout = request.CheckOutTime ?? request.CheckInTime;
        CloseActiveBreaks(attendance, effectiveCheckout);
        await ApplyAttendanceCalculationAsync(attendance, effectiveCheckout, request.CheckOutTime == null, request.Notes, cancellationToken);
        attendance.Status = request.CheckOutTime.HasValue ? AttendanceStatus.Adjusted : AttendanceStatus.MissingCheckout;

        adjustment.NewWorkedMinutes = attendance.WorkedMinutes;
        _context.AttendanceAdjustments.Add(adjustment);

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditLogAsync(adjustedByUserId, "Attendance.Adjust", nameof(DomainAttendance), attendance.AttendanceId.ToString(), oldSnapshot, attendance, ipAddress, cancellationToken);

        return await GetAttendanceDtoById(attendance.AttendanceId, cancellationToken);
    }

    public async Task<int> AutoCloseOpenShiftsAsync(DateTime? cutoffTime, int? performedByUserId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var openAttendances = await _context.Attendances
            .Include(x => x.Breaks)
            .Where(x => x.Status == AttendanceStatus.Working || x.Status == AttendanceStatus.OnBreak)
            .ToListAsync(cancellationToken);

        var updated = 0;
        foreach (var attendance in openAttendances)
        {
            var effectiveCutoff = cutoffTime ?? attendance.CheckInTime.Date.AddDays(1).AddMinutes(-1);
            if (effectiveCutoff <= attendance.CheckInTime)
            {
                effectiveCutoff = attendance.CheckInTime.AddHours(1);
            }

            CloseActiveBreaks(attendance, effectiveCutoff);
            attendance.CheckOutTime = effectiveCutoff;
            await ApplyAttendanceCalculationAsync(attendance, effectiveCutoff, true, attendance.Notes, cancellationToken);
            attendance.Status = AttendanceStatus.MissingCheckout;
            attendance.CheckOutSource ??= "AutoClose";
            attendance.UpdatedAt = DateTime.UtcNow;
            updated++;

            if (performedByUserId.HasValue)
            {
                await WriteAuditLogAsync(performedByUserId.Value, "Attendance.AutoClose", nameof(DomainAttendance), attendance.AttendanceId.ToString(), null, attendance, ipAddress, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return updated;
    }

    public async Task<AttendanceDashboardDto> GetDashboardAsync(DateTime? date, CancellationToken cancellationToken = default)
    {
        var target = (date ?? DateTime.UtcNow).Date;
        var next = target.AddDays(1);
        var attendances = await _context.Attendances
            .Where(x => x.CheckInTime >= target && x.CheckInTime < next)
            .ToListAsync(cancellationToken);

        return new AttendanceDashboardDto(
            target,
            attendances.Count(x => x.Status == AttendanceStatus.Working),
            attendances.Count(x => x.Status == AttendanceStatus.OnBreak),
            attendances.Count(x => x.Status == AttendanceStatus.Completed || x.Status == AttendanceStatus.Adjusted),
            attendances.Count(x => x.Status == AttendanceStatus.MissingCheckout),
            attendances.Sum(x => x.WorkedMinutes),
            attendances.Sum(x => x.OvertimeMinutes),
            attendances.Sum(x => x.SalaryAmount));
    }

    public async Task<IEnumerable<AttendanceEmployeeSummaryDto>> GetEmployeeSummariesAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.Attendances
            .Include(x => x.Employee)
            .Where(x => x.Status == AttendanceStatus.Completed || x.Status == AttendanceStatus.Adjusted || x.Status == AttendanceStatus.MissingCheckout)
            .AsQueryable();

        query = ApplyAttendanceFilters(query, from, to, null);
        var items = await query.ToListAsync(cancellationToken);

        return items
            .GroupBy(x => new { x.EmployeeId, x.Employee.FullName })
            .Select(g => new AttendanceEmployeeSummaryDto(
                g.Key.EmployeeId,
                g.Key.FullName,
                g.Count(),
                g.Sum(x => x.WorkedMinutes),
                g.Sum(x => x.OvertimeMinutes),
                g.Sum(x => x.SalaryAmount)))
            .OrderByDescending(x => x.WorkedMinutes)
            .ToList();
    }

    private IQueryable<DomainAttendance> ApplyAttendanceFilters(IQueryable<DomainAttendance> query, DateTime? from, DateTime? to, string? status)
    {
        if (from.HasValue)
        {
            query = query.Where(x => x.CheckInTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CheckInTime < to.Value.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AttendanceStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        return query;
    }

    private async Task EnsureNoOpenShift(int employeeId, CancellationToken cancellationToken)
    {
        var hasOpen = await _context.Attendances.AnyAsync(
            x => x.EmployeeId == employeeId && (x.Status == AttendanceStatus.Working || x.Status == AttendanceStatus.OnBreak),
            cancellationToken);

        if (hasOpen)
        {
            throw new BadRequestException("Nh�n vi�n �ang c� ca ch�a ��ng.");
        }
    }

    private async Task EnsureEmployeeExists(int employeeId, CancellationToken cancellationToken)
    {
        var exists = await _context.Users.AnyAsync(x => x.UserId == employeeId && x.IsActive, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("User", employeeId);
        }
    }

    private async Task<DomainAttendance> GetOpenAttendanceEntity(int employeeId, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .FirstOrDefaultAsync(
                x => x.EmployeeId == employeeId && (x.Status == AttendanceStatus.Working || x.Status == AttendanceStatus.OnBreak),
                cancellationToken);

        if (attendance == null)
        {
            throw new BadRequestException("Nh�n vi�n hi?n kh�ng c� ca �ang m?.");
        }

        return attendance;
    }

    private void CloseActiveBreaks(DomainAttendance attendance, DateTime endTime)
    {
        foreach (var activeBreak in attendance.Breaks.Where(x => x.Status == BreakStatus.Active))
        {
            activeBreak.EndTime = endTime;
            activeBreak.DurationMinutes = GetPositiveMinutes(activeBreak.StartTime, endTime);
            activeBreak.Status = BreakStatus.Completed;
        }
    }

    private async Task ApplyAttendanceCalculationAsync(DomainAttendance attendance, DateTime effectiveCheckout, bool missingCheckout, string? notes, CancellationToken cancellationToken)
    {
        attendance.CheckOutTime = effectiveCheckout;
        attendance.BreakMinutes = attendance.Breaks.Sum(x => x.DurationMinutes);
        attendance.WorkedMinutes = Math.Max(0, GetPositiveMinutes(attendance.CheckInTime, effectiveCheckout) - attendance.BreakMinutes);

        var rule = await ResolveSalaryRuleAsync(attendance.EmployeeId, attendance.CheckInTime, cancellationToken);
        var standardMinutes = rule.StandardHoursPerShift * 60;
        var normalMinutes = Math.Min(standardMinutes, attendance.WorkedMinutes);
        attendance.OvertimeMinutes = Math.Max(0, attendance.WorkedMinutes - standardMinutes);

        var baseSalary = (normalMinutes / 60m) * rule.HourlyRate;
        var overtimeSalary = (attendance.OvertimeMinutes / 60m) * rule.OvertimeRate;
        var nightMinutes = CalculateNightMinutes(attendance.CheckInTime, effectiveCheckout);
        var nightExtra = 0m;
        if (nightMinutes > 0 && rule.NightShiftMultiplier > 1)
        {
            nightExtra = (nightMinutes / 60m) * rule.HourlyRate * (rule.NightShiftMultiplier - 1);
        }

        attendance.SalaryAmount = Math.Round(baseSalary + overtimeSalary + nightExtra, 2, MidpointRounding.AwayFromZero);
        attendance.Status = missingCheckout ? AttendanceStatus.MissingCheckout : AttendanceStatus.Completed;
        attendance.Notes = notes ?? attendance.Notes;
    }

    private async Task<DomainSalaryRule> ResolveSalaryRuleAsync(int employeeId, DateTime effectiveDate, CancellationToken cancellationToken)
    {
        var employee = await _context.Users.FirstAsync(x => x.UserId == employeeId, cancellationToken);

        var employeeRule = await _context.SalaryRules
            .Where(x => x.EmployeeId == employeeId && x.IsActive && x.EffectiveFrom <= effectiveDate)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (employeeRule != null)
        {
            return employeeRule;
        }

        var roleRule = await _context.SalaryRules
            .Where(x => x.Role == employee.Role && x.IsActive && x.EffectiveFrom <= effectiveDate)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (roleRule == null)
        {
            throw new BadRequestException("Salary rule is missing for this employee.");
        }

        return roleRule;
    }

    private static int GetPositiveMinutes(DateTime start, DateTime end)
    {
        return Math.Max(0, (int)Math.Round((end - start).TotalMinutes, MidpointRounding.AwayFromZero));
    }

    private static int CalculateNightMinutes(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            return 0;
        }

        var minutes = 0;
        var cursor = start;
        while (cursor < end)
        {
            var next = cursor.AddMinutes(1);
            var hour = cursor.Hour;
            if (hour >= 22 || hour < 6)
            {
                minutes++;
            }
            cursor = next;
        }

        return minutes;
    }

    private async Task<AttendanceDto> GetAttendanceDtoById(int attendanceId, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .Include(x => x.Employee)
            .Include(x => x.Breaks)
            .FirstAsync(x => x.AttendanceId == attendanceId, cancellationToken);

        return MapAttendance(attendance)!;
    }

    private static AttendanceDto? MapAttendance(DomainAttendance? attendance)
    {
        if (attendance == null)
        {
            return null;
        }

        return new AttendanceDto(
            attendance.AttendanceId,
            attendance.EmployeeId,
            attendance.Employee?.FullName ?? string.Empty,
            attendance.CheckInTime,
            attendance.CheckOutTime,
            attendance.WorkedMinutes,
            attendance.BreakMinutes,
            attendance.OvertimeMinutes,
            attendance.SalaryAmount,
            attendance.Status.ToString(),
            attendance.Notes,
            attendance.Breaks
                .OrderBy(x => x.StartTime)
                .Select(x => new AttendanceBreakDto(x.BreakId, x.StartTime, x.EndTime, x.DurationMinutes, x.Status.ToString(), x.Note))
                .ToList());
    }

    private async Task WriteAuditLogAsync(int userId, string action, string entityName, string entityId, object? oldValue, object? newValue, string? ipAddress, CancellationToken cancellationToken)
    {
        _context.AuditLogs.Add(new DomainAuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValueJson = oldValue == null ? null : JsonSerializer.Serialize(oldValue),
            NewValueJson = newValue == null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}






