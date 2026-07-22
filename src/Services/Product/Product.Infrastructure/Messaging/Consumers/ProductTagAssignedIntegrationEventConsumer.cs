using System.Text.Json;

using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;
using BuildingBlock.Messaging.Abstractions;

using Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

namespace Product.Infrastructure.Messaging.Consumers;

public sealed class ProductTagAssignedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductTagAssignedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductTagAssignedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductTagAssignedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductTagAssignedIntegrationEvent");

        logger.Information("Received ProductTagAssignedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductSearchSyncRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductTagAssignedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
