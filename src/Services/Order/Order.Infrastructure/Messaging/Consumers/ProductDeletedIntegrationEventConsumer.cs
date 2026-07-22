using System.Text.Json;

using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;
using BuildingBlock.Messaging.Abstractions;

using Order.Application.Features.Catalog.Events.OnProductDeleted;

namespace Order.Infrastructure.Messaging.Consumers;

public sealed class ProductDeletedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductDeletedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductDeletedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductDeletedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductDeletedIntegrationEvent");

        logger.Information("Received ProductDeletedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductDeletedEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductDeletedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
