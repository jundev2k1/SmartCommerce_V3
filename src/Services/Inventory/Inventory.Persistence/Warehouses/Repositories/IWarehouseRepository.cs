namespace Inventory.Persistence.Warehouses.Repositories;

public interface IWarehouseRepository
{
    Task AddAsync(Warehouse entity, CancellationToken ct = default);
}
