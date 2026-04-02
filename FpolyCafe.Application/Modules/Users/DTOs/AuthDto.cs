using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Application.Modules.Users.DTOs;

public record LoginDto(string Username, string Password);
public record AuthResponseDto(string Token, string Username, string FullName, RoleType Role);
