using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

namespace SmartEcommerce.Product.Infrastructure.Messaging.Consumers;

public sealed class ProductVariationUpdatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductVariationUpdatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductVariationUpdatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductVariationUpdatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductVariationUpdatedIntegrationEvent");

        logger.Information("Received ProductVariationUpdatedIntegrationEvent for ProductVariationId: {ProductVariationId}", integrationEvent.ProductVariationId);

        var @event = new OnProductSearchSyncRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductVariationUpdatedIntegrationEvent for ProductVariationId: {ProductVariationId}", integrationEvent.ProductVariationId);
    }
}
