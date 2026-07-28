using BuildingBlock.Infrastructure.Audit;
using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Inventory.Infrastructure.BackgroundJobs;
using Inventory.Infrastructure.Messaging.Consumers;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            // Inventory now has an Outbox (added for audit tracking - Inventory/Warehouse are
            // IAuditable), so it uses the shared dual cleanup-job pair like Order/User/Audit,
            // rather than the Inbox-only registration it used when it only consumed events.
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Inventory");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "inventory-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, ProductVariationCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();

        return services;
    }
}
