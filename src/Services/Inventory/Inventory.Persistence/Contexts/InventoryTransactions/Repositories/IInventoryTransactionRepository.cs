using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
{
}
