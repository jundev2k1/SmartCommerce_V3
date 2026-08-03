using SmartEcommerce.Audit.Application.Abstractions.Services;
using SmartEcommerce.Audit.Infrastructure.BackgroundJobs;
using SmartEcommerce.Audit.Infrastructure.GrpcClients;
using SmartEcommerce.Audit.Infrastructure.Messaging.Consumers;

using SmartEcommerce.BuildingBlock.Contract.Protos.User;
using SmartEcommerce.BuildingBlock.Grpc.Client;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SmartEcommerce.Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration);

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "audit-service");
        services.AddInboxOutboxInfrastructure(configuration);

        services.AddGrpcClients(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, AuditIntegrationEventConsumer>();

        return services;
    }

    // Audit's first-ever gRPC client - see docs/tasks/2026-07-28/Task15_first-grpc-consumer.md.
    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userServiceUrl = configuration["Grpc:UserService:Url"] ?? "http://user-api:5002";

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(new Uri(userServiceUrl));
        services.AddScoped<IUserClientService, UserClientService>();

        return services;
    }
}
