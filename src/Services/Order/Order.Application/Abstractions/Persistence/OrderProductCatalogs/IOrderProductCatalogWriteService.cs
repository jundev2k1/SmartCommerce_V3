namespace Order.Application.Abstractions.Persistence.OrderProductCatalogs;

public interface IOrderProductCatalogWriteService
{
    Task CreateAsync(OrderProductCatalog entry, CancellationToken ct = default);

    /// <summary>Refreshes the full variation snapshot (name + pricing) - used when a variation-created event arrives for an id this catalog already has a row for.</summary>
    Task UpdateVariationSnapshotAsync(
        Guid productId,
        Guid variationId,
        string name,
        Sku sku,
        Money price,
        OrderProductCatalogStatus status,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default);

    Task DeleteByProductIdAsync(
        Guid productId,
        CancellationToken ct = default);
}
