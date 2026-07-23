namespace Inventory.Application.Abstractions.Persistence.InventoryTransactions;

public interface IInventoryTransactionReadService
{
    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default);
}
