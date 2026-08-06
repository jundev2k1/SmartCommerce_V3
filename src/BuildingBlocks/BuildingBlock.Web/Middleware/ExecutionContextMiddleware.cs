using System.Security.Claims;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Context;

using Microsoft.AspNetCore.Http;

namespace NovaCore.BuildingBlock.Web.Middleware;

/// <summary>
/// The only component allowed to read request identity (JWT claims, correlation/idempotency/locale
/// headers) off HttpContext. Parses it exactly once per request and hands the result to
/// ExecutionContext.Initialize - every downstream component (handlers, EF interceptors, model
/// conventions, ...) reads ExecutionContext.Current instead of touching HttpContext itself.
/// Must run after UseAuthentication/UseAuthorization (so User claims are populated) and before
/// everything else in the custom pipeline. Anonymous requests (login, refresh token, health
/// checks, public endpoints) simply get null UserId/TenantId/ScopeId - this middleware never
/// rejects a request for missing identity, that is an authorization concern, not this one's.
/// </summary>
public sealed class ExecutionContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        var data = new ExecutionContextData
        {
            UserId = TryParseGuid(user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
            TenantId = TryParseGuid(user.FindFirst(AppClaimTypes.TenantId)?.Value),
            ScopeId = TryParseGuid(user.FindFirst(AppClaimTypes.ScopeId)?.Value),
            CorrelationId = ResolveCorrelationId(context),
            IdempotencyKey = context.Request.Headers.TryGetValue(HeaderKeyConstant.IdempotencyKey, out var idempotencyKey)
                ? idempotencyKey.ToString()
                : null,
            Locale = context.Request.Headers.TryGetValue(HeaderKeyConstant.Locale, out var locale)
                ? locale.ToString()
                : null,
        };

        ExecutionContext.Initialize(data);

        await _next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderKeyConstant.CorrelationId, out var value)
            && !string.IsNullOrWhiteSpace(value))
            return value.ToString();

        return Guid.NewGuid().ToString();
    }

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var parsed) ? parsed : null;
}
