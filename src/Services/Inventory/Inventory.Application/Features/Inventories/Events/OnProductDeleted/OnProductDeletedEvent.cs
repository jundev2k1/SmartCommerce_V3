using BuildingBlock.Application.Abstractions.Events;

namespace Inventory.Application.Features.Inventories.Events.OnProductDeleted;

public sealed record OnProductDeletedEvent(Guid ProductId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
