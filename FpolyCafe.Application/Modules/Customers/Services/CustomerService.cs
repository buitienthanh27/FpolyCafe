using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Customers.DTOs;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly IAppDbContext _context;

    public CustomerService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.FullName.Contains(searchTerm) || c.PhoneNumber.Contains(searchTerm));
        }

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
            
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
        if (customer == null) throw new NotFoundException("Customer", id);
        return MapToDto(customer);
    }

    public async Task<CustomerDto> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
        if (customer == null) throw new NotFoundException("Customer with phone", phoneNumber);
        return MapToDto(customer);
    }

    public async Task<int> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (exists) throw new BadRequestException("Số điện thoại khách hàng đã tồn tại.");

        var customer = new Customer
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return customer.CustomerId;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCustomerDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
        if (customer == null) throw new NotFoundException("Customer", id);

        customer.FullName = request.FullName;
        customer.PhoneNumber = request.PhoneNumber;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
        if (customer == null) throw new NotFoundException("Customer", id);

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto(
            customer.CustomerId,
            customer.FullName,
            customer.PhoneNumber,
            customer.RewardPoints,
            customer.CreatedAt
        );
    }
}
