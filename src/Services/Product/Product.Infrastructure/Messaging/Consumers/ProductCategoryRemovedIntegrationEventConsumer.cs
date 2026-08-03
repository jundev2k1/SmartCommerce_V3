using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

namespace SmartEcommerce.Product.Infrastructure.Messaging.Consumers;

public sealed class ProductCategoryRemovedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductCategoryRemovedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductCategoryRemovedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductCategoryRemovedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductCategoryRemovedIntegrationEvent");

        logger.Information("Received ProductCategoryRemovedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductSearchSyncRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductCategoryRemovedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
