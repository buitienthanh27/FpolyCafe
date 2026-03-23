using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.AuditLogs.DTOs;

namespace FpolyCafe.Application.Modules.AuditLogs.Services;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(string? action, string? entityName, int? userId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
