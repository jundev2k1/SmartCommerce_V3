using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public sealed class InventoryTransactionRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryTransaction, Guid>(dbContext), IInventoryTransactionRepository
{
}
