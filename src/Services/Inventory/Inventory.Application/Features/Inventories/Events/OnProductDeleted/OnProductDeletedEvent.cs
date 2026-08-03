using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnProductDeleted;

public sealed record OnProductDeletedEvent(Guid ProductId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
