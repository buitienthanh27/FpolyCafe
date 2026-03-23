using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.SalaryRules.DTOs;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.SalaryRules.Services;

public class SalaryRuleService : ISalaryRuleService
{
    private readonly IAppDbContext _context;

    public SalaryRuleService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalaryRuleDto>> GetSalaryRulesAsync(int? employeeId, string? role, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _context.SalaryRules
            .Include(x => x.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<RoleType>(role, true, out var parsedRole))
        {
            query = query.Where(x => x.Role == parsedRole);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var items = await query.OrderByDescending(x => x.EffectiveFrom).ToListAsync(cancellationToken);
        return items.Select(Map);
    }

    public async Task<SalaryRuleDto> GetSalaryRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalaryRules
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.SalaryRuleId == id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SalaryRule), id);
        }

        return Map(entity);
    }

    public async Task<SalaryRuleDto> CreateSalaryRuleAsync(CreateSalaryRuleDto request, CancellationToken cancellationToken = default)
    {
        ValidateTarget(request.EmployeeId, request.Role);
        ValidateValues(request.HourlyRate, request.OvertimeRate, request.NightShiftMultiplier, request.MaxHoursPerShift, request.StandardHoursPerShift);

        var role = ParseRole(request.Role);
        if (request.EmployeeId.HasValue)
        {
            await EnsureEmployeeExists(request.EmployeeId.Value, cancellationToken);
        }

        var entity = new SalaryRule
        {
            EmployeeId = request.EmployeeId,
            Role = role,
            HourlyRate = request.HourlyRate,
            OvertimeRate = request.OvertimeRate,
            NightShiftMultiplier = request.NightShiftMultiplier,
            MaxHoursPerShift = request.MaxHoursPerShift,
            StandardHoursPerShift = request.StandardHoursPerShift,
            EffectiveFrom = request.EffectiveFrom,
            IsActive = request.IsActive
        };

        _context.SalaryRules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditLogAsync("SalaryRule.Create", entity.SalaryRuleId.ToString(), null, entity, cancellationToken);
        return await GetSalaryRuleByIdAsync(entity.SalaryRuleId, cancellationToken);
    }

    public async Task<SalaryRuleDto> UpdateSalaryRuleAsync(int id, UpdateSalaryRuleDto request, CancellationToken cancellationToken = default)
    {
        ValidateValues(request.HourlyRate, request.OvertimeRate, request.NightShiftMultiplier, request.MaxHoursPerShift, request.StandardHoursPerShift);

        var entity = await _context.SalaryRules
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.SalaryRuleId == id, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(SalaryRule), id);
        }

        var oldSnapshot = new
        {
            entity.HourlyRate,
            entity.OvertimeRate,
            entity.NightShiftMultiplier,
            entity.MaxHoursPerShift,
            entity.StandardHoursPerShift,
            entity.EffectiveFrom,
            entity.IsActive
        };

        entity.HourlyRate = request.HourlyRate;
        entity.OvertimeRate = request.OvertimeRate;
        entity.NightShiftMultiplier = request.NightShiftMultiplier;
        entity.MaxHoursPerShift = request.MaxHoursPerShift;
        entity.StandardHoursPerShift = request.StandardHoursPerShift;
        entity.EffectiveFrom = request.EffectiveFrom;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        await WriteAuditLogAsync("SalaryRule.Update", entity.SalaryRuleId.ToString(), oldSnapshot, entity, cancellationToken);
        return Map(entity);
    }

    private static void ValidateTarget(int? employeeId, string? role)
    {
        if (!employeeId.HasValue && string.IsNullOrWhiteSpace(role))
        {
            throw new BadRequestException("Salary rule ph?i áp d?ng cho employee ho?c role.");
        }
    }

    private static void ValidateValues(decimal hourlyRate, decimal overtimeRate, decimal nightShiftMultiplier, int maxHoursPerShift, int standardHoursPerShift)
    {
        if (hourlyRate <= 0 || overtimeRate <= 0)
        {
            throw new BadRequestException("M?c lýõng ph?i l?n hõn 0.");
        }

        if (nightShiftMultiplier < 1)
        {
            throw new BadRequestException("Night shift multiplier ph?i t? 1 tr? lên.");
        }

        if (standardHoursPerShift <= 0 || maxHoursPerShift <= 0 || standardHoursPerShift > maxHoursPerShift)
        {
            throw new BadRequestException("Gi? chu?n và gi? t?i ða c?a ca không h?p l?.");
        }
    }

    private static RoleType? ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        if (!Enum.TryParse<RoleType>(role, true, out var parsedRole))
        {
            throw new BadRequestException("Role không h?p l?.");
        }

        return parsedRole;
    }

    private async Task EnsureEmployeeExists(int employeeId, CancellationToken cancellationToken)
    {
        var exists = await _context.Users.AnyAsync(x => x.UserId == employeeId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("User", employeeId);
        }
    }

    private async Task WriteAuditLogAsync(string action, string entityId, object? oldValue, object? newValue, CancellationToken cancellationToken)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            EntityName = nameof(SalaryRule),
            EntityId = entityId,
            OldValueJson = oldValue == null ? null : JsonSerializer.Serialize(oldValue),
            NewValueJson = newValue == null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static SalaryRuleDto Map(SalaryRule entity)
    {
        return new SalaryRuleDto(
            entity.SalaryRuleId,
            entity.EmployeeId,
            entity.Employee?.FullName,
            entity.Role?.ToString(),
            entity.HourlyRate,
            entity.OvertimeRate,
            entity.NightShiftMultiplier,
            entity.MaxHoursPerShift,
            entity.StandardHoursPerShift,
            entity.EffectiveFrom,
            entity.IsActive);
    }
}
