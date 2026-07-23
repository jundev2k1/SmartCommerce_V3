namespace Product.Application.Abstractions.Persistence.Products;

public interface IProductReadService
{
    Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

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
