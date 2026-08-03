using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Events.OnVariantCreated;

public sealed record OnVariantCreatedEvent(
    Guid ProductId,
    Guid VariantId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
