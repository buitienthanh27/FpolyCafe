using System;

namespace FpolyCafe.Application.Modules.AuditLogs.DTOs;

public record AuditLogDto(
    int AuditLogId,
    int? UserId,
    string? UserName,
    string Action,
    string EntityName,
    string EntityId,
    string? OldValueJson,
    string? NewValueJson,
    DateTime CreatedAt,
    string? IpAddress);
