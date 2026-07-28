using BuildingBlock.Contract.Protos.Auth;
using BuildingBlock.Grpc.Client;
using BuildingBlock.Infrastructure.Audit;
using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using User.Application.Abstractions.Services;
using User.Infrastructure.BackgroundJobs;
using User.Infrastructure.Caching;
using User.Infrastructure.GrpcClients;
using User.Infrastructure.Messaging.Consumers;

namespace User.Infrastructure;

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
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddKafkaMessaging(configuration, "user-service");

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
