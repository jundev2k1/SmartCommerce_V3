using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.SharedKernel.Constants;

using Microsoft.AspNetCore.Http;

namespace BuildingBlock.Infrastructure.CurrentUser;

/// <summary>
/// Reads the caller's locale off the Accept-Language header, mirroring <see cref="CurrentUserService"/>'s
/// ambient-context shape (IHttpContextAccessor-backed, scoped, no per-endpoint boilerplate needed).
/// Only the first, bare locale tag is parsed (e.g. "en" out of "en-US,en;q=0.9") - this app has a small,
/// known set of supported locales and no server-side content-negotiation need, so a full RFC 4647
/// quality-value parser would be unused complexity.
/// </summary>
public sealed class CurrentLocaleService(IHttpContextAccessor httpContextAccessor) : ICurrentLocaleService
{
    public const string DefaultLocale = "en";

    public string GetLocale()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return DefaultLocale;

        if (!httpContext.Request.Headers.TryGetValue(HeaderKeys.Locale, out var headerValue))
            return DefaultLocale;

        var raw = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return DefaultLocale;

        var firstSegment = raw.Split(',')[0].Split(';')[0].Trim();
        return string.IsNullOrWhiteSpace(firstSegment) ? DefaultLocale : firstSegment;
    }
}
