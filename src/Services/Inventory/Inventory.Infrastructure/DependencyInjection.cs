using SmartEcommerce.BuildingBlock.Infrastructure.Audit;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.Inventory.Infrastructure.BackgroundJobs;
using SmartEcommerce.Inventory.Infrastructure.Messaging.Consumers;

namespace SmartEcommerce.Inventory.Infrastructure;

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
        services.AddScoped<IIntegrationEventConsumer, VariantCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();

        return services;
    }
}
