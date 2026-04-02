using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Auth.DTOs;

namespace FpolyCafe.Application.Modules.Auth.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
