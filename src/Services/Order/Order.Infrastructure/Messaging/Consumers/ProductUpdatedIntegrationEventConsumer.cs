using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Order.Application.Features.Catalog.Events.OnProductUpdated;

namespace SmartEcommerce.Order.Infrastructure.Messaging.Consumers;

public sealed class ProductUpdatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductUpdatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductUpdatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductUpdatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductUpdatedIntegrationEvent");

        logger.Information("Received ProductUpdatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductUpdatedEvent(integrationEvent.ProductId, integrationEvent.Name, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductUpdatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
