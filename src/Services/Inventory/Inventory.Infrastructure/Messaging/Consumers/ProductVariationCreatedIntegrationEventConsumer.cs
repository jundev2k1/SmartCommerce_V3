using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnProductVariationCreated;

namespace SmartEcommerce.Inventory.Infrastructure.Messaging.Consumers;

public sealed class ProductVariationCreatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductVariationCreatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductVariationCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        // Deliberately not caught here: the Inbox attempt executor needs the exception to
        // propagate so it can record the failure and schedule a retry.
        var integrationEvent = JsonSerializer.Deserialize<ProductVariationCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductVariationCreatedIntegrationEvent");

        logger.Information(
            "Received ProductVariationCreatedIntegrationEvent for ProductVariationId: {ProductVariationId}",
            integrationEvent.ProductVariationId);

        var @event = new OnProductVariationCreatedEvent(
            integrationEvent.ProductId,
            integrationEvent.ProductVariationId,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed ProductVariationCreatedIntegrationEvent for ProductVariationId: {ProductVariationId}",
            integrationEvent.ProductVariationId);
    }
}
