namespace Order.Application.Abstractions.Repositories;

public interface IOrderProductCatalogRepository
{
    Task<OrderProductCatalog?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<OrderProductCatalog?> GetByIdAsync(
        Guid id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        CancellationToken ct = default);

    Task<OrderProductCatalog[]> GetByVariantionIdsAsync(
        Guid[] variationIds,
        CancellationToken ct = default);

    Task AddAsync(OrderProductCatalog entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<OrderProductCatalog, Task> updateAction,
        CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        Func<OrderProductCatalog, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OrderProductCatalog>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);
}
