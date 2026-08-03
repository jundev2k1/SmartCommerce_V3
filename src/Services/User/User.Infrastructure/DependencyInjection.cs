using SmartEcommerce.BuildingBlock.Contract.Protos.Auth;
using SmartEcommerce.BuildingBlock.Grpc.Client;
using SmartEcommerce.BuildingBlock.Infrastructure.Audit;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.User.Application.Abstractions.Services;
using SmartEcommerce.User.Infrastructure.BackgroundJobs;
using SmartEcommerce.User.Infrastructure.Caching;
using SmartEcommerce.User.Infrastructure.GrpcClients;
using SmartEcommerce.User.Infrastructure.Messaging.Consumers;

namespace SmartEcommerce.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddScoped<IRoleCacheReader, RoleCacheReader>()
            .AddScoped<IUserProfileCacheService, UserProfileCacheService>()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("User");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "user-service");
        services.AddInboxOutboxInfrastructure(configuration);

        services.AddGrpcClients(configuration);

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authServiceUrl = configuration["Grpc:AuthService:Url"] ?? "http://auth-api:5002";

        // Registering Auth gRPC service
        services.AddGrpcClient<AuthGrpcService.AuthGrpcServiceClient>(new Uri(authServiceUrl));
        services.AddScoped<IAuthClientService, AuthClientService>();

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, UserAccountDeletionIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, UserProfileCreatedSearchSyncConsumer>();
        services.AddScoped<IIntegrationEventConsumer, UserProfileUpdatedSearchSyncConsumer>();

        return services;
    }
}
