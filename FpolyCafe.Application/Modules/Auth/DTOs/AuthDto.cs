using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Application.Modules.Auth.DTOs;

public record LoginRequestDto(string Username, string Password);
public record AuthResponseDto(string Token, string Username, string FullName, RoleType Role);
