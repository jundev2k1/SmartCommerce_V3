namespace SmartEcommerce.Order.Application.Features.Catalog.Events.OnProductVariationCreated;

public sealed record OnProductVariationCreatedEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string Sku,
    string ProductName,
    string VariationName,
    decimal Price,
    string Status,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
