using Auth.Application.Abstractions.Auth;
using Auth.Application.Abstractions.Services;
using Auth.Infrastructure.BackgroundJobs;
using Auth.Infrastructure.Caching;
using Auth.Infrastructure.GrpcClients;
using Auth.Infrastructure.Messaging.Consumers;
using Auth.Infrastructure.Security;
using Auth.Infrastructure.Services;

using BuildingBlock.Contract.Protos.User;
using BuildingBlock.Grpc.Client;
using BuildingBlock.Infrastructure.Audit;
using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAppLogger()
            .AddRedisCache(configuration)
            .AddRoleCaching(configuration)
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Auth")
            .AddSecurityServices()
            .AddApplicationEventDispatcher()
            .AddMessagingConsumers()
            .AddInboxOutboxInfrastructure(configuration)
            .AddKafkaMessaging(configuration, "auth-service")
            .AddGrpcClients(configuration)
            .AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, UserCreatedIntegrationEventConsumer>();

        return services;
    }

    private static IServiceCollection AddRoleCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<RoleCacheService>();

        // Decorate IAuthService with caching. This must be called AFTER
        // Persistence.AddPersistence which registers the original AuthService
        services.AddScoped<IAuthService>(provider =>
        {
            var innerAuthService = provider.GetRequiredService<AuthService>();
            var roleCache = provider.GetRequiredService<RoleCacheService>();
            return new CachedAuthServiceDecorator(innerAuthService, roleCache);
        });

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userServiceUrl = configuration["Grpc:UserService:Url"] ?? "http://user-api:5002";

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(new Uri(userServiceUrl));
        services.AddScoped<IUserProfileService, UserProfileServiceClient>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterface<IAppService>(typeof(DependencyInjection));
        return services;
    }
}
