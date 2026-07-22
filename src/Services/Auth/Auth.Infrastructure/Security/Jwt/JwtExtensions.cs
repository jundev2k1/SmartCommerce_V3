using Auth.Application.Abstractions.Security.Jwt;

using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Security.Jwt;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
