namespace Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public interface IInventoryTransactionRepository
{
    Task AddAsync(InventoryTransaction entity, CancellationToken ct = default);
}
