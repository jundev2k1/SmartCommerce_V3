namespace Product.Persistence.Contexts.Products.Repositories;

public interface IProductRepository
{
    Task<int> GetNextVariationDisplayOrderAsync(Guid productId, CancellationToken ct = default);

    Task AddVariationAsync(ProductVariation variation, CancellationToken ct = default);

    Task AddVariationRangeAsync(IEnumerable<ProductVariation> variations, CancellationToken ct = default);

    Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<ProductVariation>, IQueryable<ProductVariation>> includes,
        Func<ProductVariation, Task> updateAction,
        CancellationToken ct = default);

    Task RemoveVariationAsync(Guid id, CancellationToken ct = default);
}
