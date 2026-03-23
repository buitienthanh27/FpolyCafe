using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Modules.Payroll.Services;

public interface IPayrollService
{
    Task<IEnumerable<MonthlyPayrollDto>> GenerateMonthlyPayrollAsync(GeneratePayrollRequestDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<MonthlyPayrollDto>> GetMonthlyPayrollsAsync(int month, int year, CancellationToken cancellationToken = default);
    Task<MonthlyPayrollDto> GetEmployeePayrollAsync(int employeeId, int month, int year, CancellationToken cancellationToken = default);
}
