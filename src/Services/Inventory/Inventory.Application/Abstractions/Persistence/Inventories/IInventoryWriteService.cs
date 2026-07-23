namespace Inventory.Application.Abstractions.Persistence.Inventories;

public interface IInventoryWriteService
{
    /// <summary>Self-committing (ExecuteTransactionAsync). Used by OnProductVariationCreated - the only write in its transaction.</summary>
    Task AddAsync(InventoryEntity entity, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnProductDeleted - the only write in its transaction.</summary>
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnProductVariationDeleted - the only write in its transaction.</summary>
    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: loads the tracked entity and applies updateAction, but does not call
    /// IUnitOfWork itself. Every caller (AdjustStock/StockIn/StockOut/DeductStock/RestockStock)
    /// also mutates InventoryTransaction (and StockDeduction, for Deduct/Restock) atomically and
    /// owns the ExecuteTransactionAsync call itself - see Correction 2 in the persistence
    /// refactor tracker.
    /// </summary>
    Task StageUpdateAsync(Guid id, Func<InventoryEntity, Task> updateAction, CancellationToken ct = default);
}
