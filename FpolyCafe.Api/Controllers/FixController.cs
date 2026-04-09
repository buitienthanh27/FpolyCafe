using FpolyCafe.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FpolyCafe.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixController : ControllerBase
{
    private readonly IAppDbContext _context;

    public FixController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet("passwords")]
    public async Task<IActionResult> FixPasswords()
    {
        var users = await _context.Users.ToListAsync();
        int fixedCount = 0;
        var details = users.Select(u => new { u.Username, Hash = u.PasswordHash }).ToList();
        foreach (var user in users)
        {
            if (!user.PasswordHash.StartsWith("$2a$") && !user.PasswordHash.StartsWith("$2b$") && !user.PasswordHash.StartsWith("$2y$"))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                fixedCount++;
            }
        }
        await _context.SaveChangesAsync(default);
        return Ok(new { message = $"Fixed {fixedCount} passwords.", total = users.Count, details });
    }
}
