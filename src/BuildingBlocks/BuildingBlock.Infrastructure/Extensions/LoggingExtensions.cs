#pragma warning disable CS1591

using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Infrastructure.Logging;

using Microsoft.Extensions.DependencyInjection;

namespace SmartEcommerce.BuildingBlock.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static IServiceCollection AddAppLogger(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}
