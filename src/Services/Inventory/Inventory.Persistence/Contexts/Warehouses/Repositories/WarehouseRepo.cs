using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Warehouses.Repositories;

public sealed class WarehouseRepo(InventoryDbContext dbContext)
    : InventoryBaseRepository<Warehouse, Guid>(dbContext), IWarehouseRepository
{
}
