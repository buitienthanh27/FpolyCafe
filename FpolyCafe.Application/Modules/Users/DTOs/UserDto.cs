namespace FpolyCafe.Application.Modules.Users.DTOs;

public record UserDto(int UserId, string Username, string FullName, string Email, string Role, bool IsActive);
public record CreateUserDto(string Username, string Password, string FullName, string Email, string Role);
public record UpdateUserDto(string FullName, string Email, string Role, bool IsActive);
