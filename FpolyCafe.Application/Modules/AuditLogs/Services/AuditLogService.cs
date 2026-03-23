using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.AuditLogs.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAppDbContext _context;

    public AuditLogService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(string? action, string? entityName, int? userId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action.Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName.Contains(entityName));
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt < to.Value.AddDays(1));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return items.Select(x => new AuditLogDto(
            x.AuditLogId,
            x.UserId,
            x.User?.FullName,
            x.Action,
            x.EntityName,
            x.EntityId,
            x.OldValueJson,
            x.NewValueJson,
            x.CreatedAt,
            x.IpAddress));
    }
}
