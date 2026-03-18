using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FpolyCafe.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FpolyCafe.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = notFoundException.Message });
                break;
            case BadRequestException badRequestException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = badRequestException.Message });
                break;
            case ForbiddenException:
                code = HttpStatusCode.Forbidden;
                result = JsonSerializer.Serialize(new { error = "You do not have permission to perform this action." });
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = "Unauthorized access." });
                break;
            default:
                result = JsonSerializer.Serialize(new { error = "An internal server error occurred." });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
