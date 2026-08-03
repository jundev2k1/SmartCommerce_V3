using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using Microsoft.AspNetCore.Http;

namespace SmartEcommerce.BuildingBlock.Web.CurrentUser;

public sealed class CurrentLocaleService(IHttpContextAccessor httpContextAccessor) : ICurrentLocaleService
{
    public const string DefaultLocale = "en";

    public string GetLocale()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return DefaultLocale;

        if (!httpContext.Request.Headers.TryGetValue(HeaderKeyConstant.Locale, out var headerValue))
            return DefaultLocale;

        var raw = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return DefaultLocale;

        var firstSegment = raw.Split(',')[0].Split(';')[0].Trim();
        return string.IsNullOrWhiteSpace(firstSegment) ? DefaultLocale : firstSegment;
    }
}
