namespace Inventory.Application.Abstractions.Persistence.Inventories;

public sealed record InventoryAdjustmentResult(InventoryEntity Entity, int Delta);

public interface IInventoryWriteService
{
    /// <summary>Self-committing (ExecuteTransactionAsync). Used by OnProductVariationCreated - the only write in its transaction.</summary>
    Task AddAsync(InventoryEntity entity, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnProductDeleted - the only write in its transaction.</summary>
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Self-committing. Used by OnProductVariationDeleted - the only write in its transaction.</summary>
    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: increases stock and returns the updated entity. Callers (StockIn/RestockStock)
    /// also stage an InventoryTransaction and own the ExecuteTransactionAsync call themselves - see
    /// Correction 2 in the persistence refactor tracker.
    /// </summary>
    Task<InventoryEntity> IncreaseAsync(Guid id, int amount, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: decreases stock (guards against overselling) and returns the updated entity.
    /// Callers (DeductStock/StockOut) also stage an InventoryTransaction and own the
    /// ExecuteTransactionAsync call themselves.
    /// </summary>
    Task<InventoryEntity> DecreaseAsync(Guid id, int amount, CancellationToken ct = default);

    /// <summary>
    /// Non-committing: corrects stock to newQuantity (e.g. after a physical count) and returns the
    /// updated entity plus the delta applied, for transaction logging. Caller (AdjustStock) also
    /// stages an InventoryTransaction and owns the ExecuteTransactionAsync call itself.
    /// </summary>
    Task<InventoryAdjustmentResult> AdjustToAsync(Guid id, int newQuantity, CancellationToken ct = default);
}
