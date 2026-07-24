using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Warehouses.Read;

public sealed class WarehouseReadService(InventoryDbContext dbContext) : IWarehouseReadService
{
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, ct);
    }
}
