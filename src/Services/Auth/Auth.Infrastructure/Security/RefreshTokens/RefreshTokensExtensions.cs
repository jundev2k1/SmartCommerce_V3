using Auth.Application.Abstractions.Services;
using Auth.Infrastructure.Caching;
using Auth.Infrastructure.Security.RefreshTokens.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Security.RefreshTokens;

public static class RefreshTokensExtensions
{
    public static IServiceCollection AddRefreshTokens(this IServiceCollection services)
    {
        services.AddSingleton<RefreshTokenCacheService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRefreshTokenInitializationService, RefreshTokenInitializationService>();
        return services;
    }
}
