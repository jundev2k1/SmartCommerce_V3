namespace Order.Application.Abstractions.Persistence.OrderProductCatalogs;

public interface IOrderProductCatalogWriteService
{
    Task CreateAsync(OrderProductCatalog entry, CancellationToken ct = default);

    /// <summary>Refreshes the full variation snapshot (name + pricing) - used when a variation-created event arrives for an id this catalog already has a row for.</summary>
    Task UpdateVariationSnapshotAsync(
        Guid id, string productName, string sku, decimal price, string status, CancellationToken ct = default);

    /// <summary>Refreshes pricing/status only, not the product name - used when a variation is updated in place.</summary>
    Task UpdatePricingAsync(Guid id, string sku, decimal price, string status, CancellationToken ct = default);

    /// <summary>A Product's name is shared across every one of its variations, so this refreshes every catalog row for the product, not just one.</summary>
    Task UpdateProductNameByProductIdAsync(Guid productId, string productName, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);
}
