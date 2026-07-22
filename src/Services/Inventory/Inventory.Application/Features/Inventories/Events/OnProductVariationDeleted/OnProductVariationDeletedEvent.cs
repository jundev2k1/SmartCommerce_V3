using BuildingBlock.Application.Abstractions.Events;

namespace Inventory.Application.Features.Inventories.Events.OnProductVariationDeleted;

public sealed record OnProductVariationDeletedEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
