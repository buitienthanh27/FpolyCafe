using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AppBadRequestException = FpolyCafe.Application.Common.Exceptions.BadRequestException;
using AppNotFoundException = FpolyCafe.Application.Common.Exceptions.NotFoundException;
using AppUnauthorizedException = FpolyCafe.Application.Common.Exceptions.UnauthorizedException;
using DomainBadRequestException = FpolyCafe.Domain.Exceptions.BadRequestException;
using DomainForbiddenException = FpolyCafe.Domain.Exceptions.ForbiddenException;
using DomainNotFoundException = FpolyCafe.Domain.Exceptions.NotFoundException;
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
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        object payload = new { code = "internal_error", message = "An internal server error occurred." };

        switch (exception)
        {
            case AppNotFoundException or DomainNotFoundException:
                code = HttpStatusCode.NotFound;
                payload = new { code = "not_found", message = exception.Message };
                break;
            case AppBadRequestException or DomainBadRequestException:
                code = HttpStatusCode.BadRequest;
                payload = new { code = "bad_request", message = exception.Message };
                break;
            case DomainForbiddenException:
                code = HttpStatusCode.Forbidden;
                payload = new { code = "forbidden", message = exception.Message };
                break;
            case AppUnauthorizedException or UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                payload = new { code = "unauthorized", message = exception.Message };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        if (code == HttpStatusCode.InternalServerError)
        {
            payload = new { code = "internal_error", message = "MODIFIED: Internal Server Error Test - " + exception.Message, stackTrace = exception.StackTrace };
        }
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
