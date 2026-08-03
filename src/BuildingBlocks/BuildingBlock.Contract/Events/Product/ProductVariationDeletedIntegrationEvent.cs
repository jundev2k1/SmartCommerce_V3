namespace SmartEcommerce.BuildingBlock.Contract.Events.Product;

public sealed record ProductVariationDeletedIntegrationEvent(
    Guid ProductId,
    Guid ProductVariationId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductVariationDeletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
