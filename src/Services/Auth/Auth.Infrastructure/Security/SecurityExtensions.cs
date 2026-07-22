using Auth.Infrastructure.Security.Jwt;
using Auth.Infrastructure.Security.RefreshTokens;

using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services
            .AddJwtServices()
            .AddRefreshTokens();

        return services;
    }
}
