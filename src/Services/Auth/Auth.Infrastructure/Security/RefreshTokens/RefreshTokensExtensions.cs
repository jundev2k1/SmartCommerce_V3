using SmartEcommerce.Auth.Application.Abstractions.Services;
using SmartEcommerce.Auth.Infrastructure.Caching;
using SmartEcommerce.Auth.Infrastructure.Security.RefreshTokens.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace SmartEcommerce.Auth.Infrastructure.Security.RefreshTokens;

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
