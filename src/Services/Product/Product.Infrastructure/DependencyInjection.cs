using SmartEcommerce.BuildingBlock.Contract.Protos.Inventory;
using SmartEcommerce.BuildingBlock.Grpc.Client;
using SmartEcommerce.BuildingBlock.Infrastructure.Audit;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;

using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.Product.Application.Abstractions.Services;
using SmartEcommerce.Product.Infrastructure.BackgroundJobs;
using SmartEcommerce.Product.Infrastructure.GrpcClients;
using SmartEcommerce.Product.Infrastructure.Messaging.Consumers;

namespace SmartEcommerce.Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Product");

        services.AddApplicationEventDispatcher();
        services.AddGrpcClients(configuration);

        // Consumers must be registered before AddKafkaMessaging - their Topics are discovered
        // eagerly to configure the KafkaFlow consumer pipeline. AddInboxOutboxInfrastructure
        // must come after AddKafkaMessaging so its real Inbox delegate registration is last
        // and wins over AddKafkaMessaging's placeholder.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "product-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var inventoryServiceUrl = configuration["Grpc:InventoryService:Url"] ?? "http://inventory-api:5002";

        services.AddGrpcClient<InventoryGrpcService.InventoryGrpcServiceClient>(new Uri(inventoryServiceUrl));
        services.AddScoped<IInventoryClientService, InventoryClientService>();

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
