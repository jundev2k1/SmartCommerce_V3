#pragma warning disable CS1591

using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Infrastructure.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddAppLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}
