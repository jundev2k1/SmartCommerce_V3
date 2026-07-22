namespace Product.Application.Abstractions.Repositories;

/// <summary>
/// Product owns its ProductVariation collection (EF owned type - always loaded with the
/// aggregate, see Product.Persistence/Configs/ProductConfig.cs), so unlike the pre-redesign
/// version this repository no longer needs a conditional-include overload. The shape no longer
/// matches BuildingBlock.Persistence.Repository.IRepository&lt;T&gt; (custom Search/Exists methods,
/// no includes overload), so ProductRepo is registered manually in DI instead of being picked up
/// by the generic IRepository&lt;&gt; Scrutor scan - same reasoning as Inventory's
/// IInventoryTransactionRepository / Audit's IAuditLogRepository.
/// </summary>
public interface IProductRepository
{
    Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(ProductEntity entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<ProductEntity, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);

    Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default);

    Task<bool> SkuExistsAsync(string sku, Guid? excludeVariationId = null, CancellationToken ct = default);

    /// <summary>Name of the product currently owning a variation with this SKU, if any - used only to enrich the 409 conflict message with which product the caller is colliding with (SKU uniqueness is global, not per-product).</summary>
    Task<string?> GetProductNameBySkuAsync(string sku, CancellationToken ct = default);

    Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default);

    Task<bool> ExistsWithTagAsync(Guid tagId, CancellationToken ct = default);

    /// <summary>Full-catalog enumeration (no filters), used only by RebuildProductSearchIndexHandler to page the whole Postgres catalog into Elasticsearch.</summary>
    Task<IReadOnlyList<ProductEntity>> GetAllAsync(int skip, int take, CancellationToken ct = default);
}
