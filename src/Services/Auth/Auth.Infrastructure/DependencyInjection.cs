using SmartEcommerce.Auth.Application.Abstractions.Auth;
using SmartEcommerce.Auth.Application.Abstractions.Services;
using SmartEcommerce.Auth.Infrastructure.BackgroundJobs;
using SmartEcommerce.Auth.Infrastructure.Caching;
using SmartEcommerce.Auth.Infrastructure.GrpcClients;
using SmartEcommerce.Auth.Infrastructure.Messaging.Consumers;
using SmartEcommerce.Auth.Infrastructure.Security;
using SmartEcommerce.Auth.Infrastructure.Services;

using SmartEcommerce.BuildingBlock.Contract.Protos.User;
using SmartEcommerce.BuildingBlock.Grpc.Client;
using SmartEcommerce.BuildingBlock.Infrastructure.Audit;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartEcommerce.Auth.Infrastructure;

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
            .AddKafkaMessaging(configuration, "auth-service")
            .AddInboxOutboxInfrastructure(configuration)
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
