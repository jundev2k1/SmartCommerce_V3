using SmartEcommerce.BuildingBlock.Contract.Protos.Inventory;
using SmartEcommerce.BuildingBlock.Grpc.Client;
using SmartEcommerce.BuildingBlock.Infrastructure.Audit;
using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;
using SmartEcommerce.BuildingBlock.Saga.Abstractions;
using SmartEcommerce.BuildingBlock.Saga.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.Order.Application.Abstractions.Services;
using SmartEcommerce.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;
using SmartEcommerce.Order.Infrastructure.BackgroundJobs;
using SmartEcommerce.Order.Infrastructure.Caching;
using SmartEcommerce.Order.Infrastructure.GrpcClients;
using SmartEcommerce.Order.Infrastructure.Messaging.Consumers;

namespace SmartEcommerce.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddScoped<ICartService, CartService>()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Order");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // gRPC client + saga orchestrator registration: no ordering constraint relative to
        // AddKafkaMessaging - OrderCreatedSagaConsumer (registered below) only depends on
        // ISender/IAppLogger, so AddKafkaMessaging's DiscoverConsumerTopics (which eagerly
        // constructs every registered consumer to read its Topics) never touches these. The
        // actual saga steps/orchestrator are resolved lazily by MediatR inside
        // RunCreateOrderSagaHandler, only when a message is really processed - see
        // docs/reference/create-order-saga.md.
        services.AddGrpcClients(configuration);
        services.AddCreateOrderSaga();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "order-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, ProductVariationCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductVariationDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();

        // Drives CreateOrderSaga - see docs/reference/create-order-saga.md.
        services.AddScoped<IIntegrationEventConsumer, OrderCreatedSagaConsumer>();

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
    /// Steps are Scoped (they depend on Scoped repos/gRPC clients); the orchestrator itself is
    /// Singleton to match SmartEcommerce.BuildingBlock.Saga.Extensions.SagaExtensions' own convention - it never
    /// caches steps/definitions between calls, so this is safe (see OrderCreatedSagaConsumer,
    /// which resolves the Scoped steps itself and passes them in per message). ISagaStore is
    /// registered separately in SmartEcommerce.Order.Persistence (EfSagaStore).
    /// </summary>
    private static IServiceCollection AddCreateOrderSaga(this IServiceCollection services)
    {
        services.AddScoped<DeductInventoryStep>();
        services.AddScoped<ConfirmOrderStep>();
        services.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();

        return services;
    }
}
