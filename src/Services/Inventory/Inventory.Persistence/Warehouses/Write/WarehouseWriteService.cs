using BuildingBlock.Application.Abstractions.Persistence;

using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Persistence.Warehouses.Repositories;

namespace Inventory.Persistence.Warehouses.Write;

public sealed class WarehouseWriteService(
    IWarehouseRepository repo,
    IUnitOfWork unitOfWork) : IWarehouseWriteService
{
    public async Task CreateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        await repo.AddAsync(warehouse, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
