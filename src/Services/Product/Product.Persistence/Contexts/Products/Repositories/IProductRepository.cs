using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Product.Persistence.Contexts.Products.Repositories;

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
        Variant variation,
        CancellationToken ct = default);

    Task AddVariationRangeAsync(
        IEnumerable<Variant> variations,
        CancellationToken ct = default);

    Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<Variant>, IQueryable<Variant>> includes,
        Func<Variant, Task> updateAction,
        CancellationToken ct = default);

    Task RemoveVariationAsync(Guid id, CancellationToken ct = default);
}
