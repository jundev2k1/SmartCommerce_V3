using SmartEcommerce.BuildingBlock.Application;
using FluentValidation;

using Mapster;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.User.Application.Abstractions.Services;
using SmartEcommerce.User.Application.Features.Users.Caching;
using SmartEcommerce.User.Application.Features.Users.Search;
using SmartEcommerce.User.Application.Services;

namespace SmartEcommerce.User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddMediatR()
            .AddApplicationBehaviors()
            .AddMapster()
            .AddFluentValidation()
            .AddSingleton<IUserDisplayNameFormatter, UserDisplayNameFormatter>()
            .AddScoped<UserSearchProjectionBuilder>()
            .AddScoped<CachedUserProfileReader>();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(DependencyInjection).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
