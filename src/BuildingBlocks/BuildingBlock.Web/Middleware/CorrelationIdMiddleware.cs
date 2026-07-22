using System.Diagnostics;

using BuildingBlock.SharedKernel.Constants;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Web.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderKeys.CorrelationId, out var value)
            && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

        Activity.Current?.SetTag("correlationId", correlationId);

        using (logger.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
