using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.Warehouses.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
