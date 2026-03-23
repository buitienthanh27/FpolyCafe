using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Users.DTOs;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;

    public UserService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Select(u => new UserDto(u.UserId, u.Username, u.FullName, "", u.Role.ToString(), u.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(User), id);

        return new UserDto(user.UserId, user.Username, user.FullName, "", user.Role.ToString(), user.IsActive);
    }

    public async Task<int> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (exists)
            throw new BadRequestException("Username đã tồn tại.");

        if (!Enum.TryParse<RoleType>(request.Role, true, out var parsedRole))
            throw new BadRequestException("Role không hợp lệ.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = parsedRole,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.UserId;
    }

    public async Task<bool> UpdateUserAsync(int userId, UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(User), userId);

        if (!Enum.TryParse<RoleType>(request.Role, true, out var parsedRole))
            throw new BadRequestException("Role không hợp lệ.");

        user.FullName = request.FullName;
        user.Role = parsedRole;
        user.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(User), userId);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
