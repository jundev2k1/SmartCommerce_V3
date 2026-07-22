using System.Text.Json;

using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;
using BuildingBlock.Messaging.Abstractions;

using Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

namespace Product.Infrastructure.Messaging.Consumers;

/// <summary>Product self-consumes its own integration events to keep the Search index in sync - see docs/reference/search.md.</summary>
public sealed class ProductCreatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductCreatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductCreatedIntegrationEvent");

        logger.Information("Received ProductCreatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductSearchSyncRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductCreatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
