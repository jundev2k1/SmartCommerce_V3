namespace Inventory.Persistence.Warehouses.Repositories;

public sealed class WarehouseRepo(InventoryDbContext dbContext) : IWarehouseRepository
{
    public async Task AddAsync(Warehouse entity, CancellationToken ct = default)
    {
        await dbContext.Warehouses.AddAsync(entity, ct);
    }
}
