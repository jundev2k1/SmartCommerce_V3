using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnVariantDeleted;

public sealed record OnVariantDeletedEvent(
    Guid ProductId,
    Guid VariantId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
