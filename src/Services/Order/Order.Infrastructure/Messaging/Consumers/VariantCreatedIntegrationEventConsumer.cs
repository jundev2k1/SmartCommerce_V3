using System.Text.Json;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;

using SmartEcommerce.Order.Application.Features.Catalog.Events.OnVariantCreated;

namespace SmartEcommerce.Order.Infrastructure.Messaging.Consumers;

public sealed class VariantCreatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<VariantCreatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(VariantCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<VariantCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize VariantCreatedIntegrationEvent");

        logger.Information(
            "Received VariantCreatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);

        var @event = new OnVariantCreatedEvent(
            integrationEvent.ProductId,
            integrationEvent.VariantId,
            integrationEvent.Sku,
            integrationEvent.ProductName,
            integrationEvent.VariationName,
            integrationEvent.Price,
            integrationEvent.Status,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed VariantCreatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);
    }
}
