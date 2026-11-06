using FoodBridge.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FoodBridge.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            UserBlockedException => (StatusCodes.Status403Forbidden, "Forbidden"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            DomainException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = $"https://httpstatuses.com/{status}",
                title,
                status,
                detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,
                traceId = httpContext.TraceIdentifier
            },
            cancellationToken);

        return true;
    }
}
