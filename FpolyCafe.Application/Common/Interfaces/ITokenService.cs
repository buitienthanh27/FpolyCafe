using FpolyCafe.Domain.Entities;

namespace FpolyCafe.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
