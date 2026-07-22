namespace Inventory.Application.Abstractions.Repositories;

public interface IInventoryRepository
{
    Task<InventoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<InventoryEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<InventoryEntity>, IQueryable<InventoryEntity>> includes,
        CancellationToken ct = default);

    /// <summary>Stock is now keyed at (ProductVariationId, WarehouseId) - see Inventory entity remarks.</summary>
    Task<InventoryEntity?> GetByVariationAndWarehouseAsync(
        Guid productVariationId,
        Guid warehouseId,
        CancellationToken ct = default);

    Task AddAsync(InventoryEntity entity, CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<InventoryEntity, Task> updateAction,
        CancellationToken ct = default);

    Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<InventoryEntity>, IQueryable<InventoryEntity>> includes,
        Func<InventoryEntity, Task> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryEntity>> GetByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Rollup across every variation and warehouse for the whole product.</summary>
    Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Rollup across every warehouse for a single variation.</summary>
    Task<int> GetTotalStockByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>Batched rollup across every warehouse for each requested variation, in one query. Variation ids with no inventory rows are absent from the result (callers should default to 0).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default);
}
