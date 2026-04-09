using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Customers.DTOs;

namespace FpolyCafe.Application.Modules.Customers.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateCustomerDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
