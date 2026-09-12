using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

namespace FitCore.Api.Errors.Exceptions;

public sealed class EmailExceptionHandler(ILogger<EmailExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not EmailDeliveryException)
            return false;

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        logger.LogError(
            exception,
            "Email delivery failed for {Method} {Path} (user {UserId})",
            httpContext.Request.Method,
            httpContext.Request.Path,
            userId ?? "anonymous");

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        await httpContext.Response.WriteAsJsonAsync(
            new { message = "Email service is temporarily unavailable. Try again." },
            cancellationToken);

        return true;
    }
}
