using BuildingBlock.Infrastructure.Audit;
using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;

using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Product.Infrastructure.BackgroundJobs;
using Product.Infrastructure.Messaging.Consumers;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Product");

        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are discovered
        // eagerly to configure the KafkaFlow consumer pipeline. The Outbox relay/Inbox delegates
        // must also be in place first, same ordering constraint as every other consuming service.
        services.AddMessagingConsumers();
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddKafkaMessaging(configuration, "product-service");

        return services;
    }

    /// <summary>
    /// Product now both publishes and self-consumes (Search index sync, see
    /// docs/reference/search.md) - one consumer per Product integration event, each a thin
    /// adapter dispatching an internal Search-sync/removal event, no business logic here.
    /// </summary>
    private static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, ProductCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductCategoryAssignedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductCategoryRemovedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductTagAssignedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductTagRemovedIntegrationEventConsumer>();
        return services;
    }
}
