namespace SmartEcommerce.Order.Application.Features.Catalog.Events.OnProductVariationUpdated;

public sealed record OnProductVariationUpdatedEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string Name,
    string Sku,
    decimal Price,
    string Status,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
