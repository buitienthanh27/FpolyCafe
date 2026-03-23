using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.SalaryRules.DTOs;

namespace FpolyCafe.Application.Modules.SalaryRules.Services;

public interface ISalaryRuleService
{
    Task<IEnumerable<SalaryRuleDto>> GetSalaryRulesAsync(int? employeeId, string? role, bool? isActive, CancellationToken cancellationToken = default);
    Task<SalaryRuleDto> GetSalaryRuleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SalaryRuleDto> CreateSalaryRuleAsync(CreateSalaryRuleDto request, CancellationToken cancellationToken = default);
    Task<SalaryRuleDto> UpdateSalaryRuleAsync(int id, UpdateSalaryRuleDto request, CancellationToken cancellationToken = default);
}
