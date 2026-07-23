namespace Inventory.Application.Abstractions.Persistence.InventoryTransactions;

public interface IInventoryTransactionWriteService
{
    /// <summary>
    /// Non-committing: every caller batches this into a larger cross-aggregate transaction it
    /// owns itself (Inventory + InventoryTransaction, plus StockDeduction for Deduct/Restock) -
    /// see Correction 2 in the persistence refactor tracker.
    /// </summary>
    Task StageAddAsync(InventoryTransaction entity, CancellationToken ct = default);
}
