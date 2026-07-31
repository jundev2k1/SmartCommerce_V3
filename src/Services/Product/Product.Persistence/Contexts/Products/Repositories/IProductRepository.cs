using BuildingBlock.Persistence.Repository;

namespace Product.Persistence.Contexts.Products.Repositories;

public interface IProductRepository : IRepository<ProductEntity>
{
    Task<ProductEntity[]> GetAllAsync(
        int skip,
        int take,
        CancellationToken ct = default);

    Task<Guid[]> GetProductsByTagIdAsync(
        Guid tagId,
        CancellationToken ct = default);

    Task<int> GetNextVariationDisplayOrderAsync(
        Guid productId,
        CancellationToken ct = default);

    Task AddVariationAsync(
        ProductVariation variation,
        CancellationToken ct = default);

    Task AddVariationRangeAsync(
        IEnumerable<ProductVariation> variations,
        CancellationToken ct = default);

    Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<ProductVariation>, IQueryable<ProductVariation>> includes,
        Func<ProductVariation, Task> updateAction,
        CancellationToken ct = default);

    Task RemoveVariationAsync(Guid id, CancellationToken ct = default);
}
