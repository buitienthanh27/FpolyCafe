using System;

namespace FpolyCafe.Application.Modules.SalaryRules.DTOs;

public record SalaryRuleDto(
    int SalaryRuleId,
    int? EmployeeId,
    string? EmployeeName,
    string? Role,
    decimal HourlyRate,
    decimal OvertimeRate,
    decimal NightShiftMultiplier,
    int MaxHoursPerShift,
    int StandardHoursPerShift,
    DateTime EffectiveFrom,
    bool IsActive);

public record CreateSalaryRuleDto(
    int? EmployeeId,
    string? Role,
    decimal HourlyRate,
    decimal OvertimeRate,
    decimal NightShiftMultiplier,
    int MaxHoursPerShift,
    int StandardHoursPerShift,
    DateTime EffectiveFrom,
    bool IsActive);

public record UpdateSalaryRuleDto(
    decimal HourlyRate,
    decimal OvertimeRate,
    decimal NightShiftMultiplier,
    int MaxHoursPerShift,
    int StandardHoursPerShift,
    DateTime EffectiveFrom,
    bool IsActive);
