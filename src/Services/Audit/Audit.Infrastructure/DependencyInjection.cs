using Audit.Application.Abstractions.Services;
using Audit.Infrastructure.BackgroundJobs;
using Audit.Infrastructure.GrpcClients;
using Audit.Infrastructure.Messaging.Consumers;

using BuildingBlock.Contract.Protos.User;
using BuildingBlock.Grpc.Client;
using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

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
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddKafkaMessaging(configuration, "audit-service");

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
