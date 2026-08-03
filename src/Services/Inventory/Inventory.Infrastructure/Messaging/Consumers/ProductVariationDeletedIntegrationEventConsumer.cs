using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnProductVariationDeleted;

namespace SmartEcommerce.Inventory.Infrastructure.Messaging.Consumers;

public sealed class ProductVariationDeletedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductVariationDeletedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductVariationDeletedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductVariationDeletedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductVariationDeletedIntegrationEvent");

        logger.Information(
            "Received ProductVariationDeletedIntegrationEvent for ProductVariationId: {ProductVariationId}",
            integrationEvent.ProductVariationId);

        var @event = new OnProductVariationDeletedEvent(
            integrationEvent.ProductId,
            integrationEvent.ProductVariationId,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed ProductVariationDeletedIntegrationEvent for ProductVariationId: {ProductVariationId}",
            integrationEvent.ProductVariationId);
    }
}
