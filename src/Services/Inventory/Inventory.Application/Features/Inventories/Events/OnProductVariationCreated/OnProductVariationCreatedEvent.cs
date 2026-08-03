using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnProductVariationCreated;

public sealed record OnProductVariationCreatedEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
