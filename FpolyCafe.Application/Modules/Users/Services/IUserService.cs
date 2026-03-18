using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Users.DTOs;

namespace FpolyCafe.Application.Modules.Users.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateUserAsync(CreateUserDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto request, CancellationToken cancellationToken = default);
}
