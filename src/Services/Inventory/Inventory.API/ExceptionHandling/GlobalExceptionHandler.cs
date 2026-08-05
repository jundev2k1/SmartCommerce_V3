using NovaCore.BuildingBlock.Infrastructure.ExceptionHandling;

using Microsoft.AspNetCore.Diagnostics;

namespace NovaCore.Inventory.API.ExceptionHandling;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct = default)
    {
        var result = ExceptionHandlerHelper.HandleException(exception);

        logger.LogError(
            exception,
            "[{StatusCode}] {LogMessage}",
            result.StatusCode,
            result.LogMessage);

        httpContext.Response.StatusCode = result.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(result.ApiResponse, ct);

        return true;
    }
}
