using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Auth.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(IAppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Username hoặc Password không đúng.");
        }

        var token = _tokenService.CreateToken(user);
        return new AuthResponseDto(token, user.Username, user.FullName, user.Role);
    }
}
