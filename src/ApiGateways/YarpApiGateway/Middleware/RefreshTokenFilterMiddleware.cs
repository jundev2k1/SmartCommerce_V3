using StackExchange.Redis;

using SmartEcommerce.YarpApiGateway.Caching;
using SmartEcommerce.YarpApiGateway.Configuration;

namespace SmartEcommerce.YarpApiGateway.Middleware;

/// <summary>
/// Rejects refresh-token requests early when the token isn't present in the distributed cache,
/// so obviously invalid refresh attempts never reach SmartEcommerce.Auth.API. Mirrors SmartEcommerce.Auth.API's own design:
/// the refresh token itself is the Redis key, and Redis is the source of truth for its validity.
/// </summary>
public sealed class RefreshTokenFilterMiddleware(RequestDelegate next, GatewayOptions options, IConnectionMultiplexer redis)
{
    private readonly RequestDelegate _next = next;
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly string _refreshTokenPath = BuildRefreshTokenPath(options);

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsRefreshTokenRequest(context))
        {
            if (!context.Request.Cookies.TryGetValue("RefreshToken", out var token) ||
                string.IsNullOrWhiteSpace(token) ||
                !await _redis.RefreshTokenExistsAsync(token, context.RequestAborted))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = "Invalid or expired refresh token.",
                    MessageCode = "302"
                });
                return;
            }
        }

        await _next(context);
    }

    private bool IsRefreshTokenRequest(HttpContext context) =>
        HttpMethods.IsPost(context.Request.Method) &&
        string.Equals(context.Request.Path.Value?.TrimEnd('/'), _refreshTokenPath, StringComparison.OrdinalIgnoreCase);

    private static string BuildRefreshTokenPath(GatewayOptions options)
    {
        var authPath = options.Services.TryGetValue("Auth", out var auth) ? auth.Path : "/api/auth/";
        return $"{authPath.TrimEnd('/')}/refresh-token";
    }
}

public static class RefreshTokenFilterMiddlewareExtensions
{
    public static IApplicationBuilder UseRefreshTokenFilter(this IApplicationBuilder app)
        => app.UseMiddleware<RefreshTokenFilterMiddleware>();
}
