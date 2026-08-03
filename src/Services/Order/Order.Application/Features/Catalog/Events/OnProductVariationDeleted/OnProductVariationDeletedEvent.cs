namespace SmartEcommerce.Order.Application.Features.Catalog.Events.OnProductVariationDeleted;

public sealed record OnProductVariationDeletedEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
