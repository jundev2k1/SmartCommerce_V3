using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.Warehouses;

namespace Inventory.Persistence.Contexts.Warehouses.Write;

public sealed class WarehouseWriteService(
    IRepository<Warehouse> repo,
    IUnitOfWork unitOfWork) : IWarehouseWriteService
{
    public async Task CreateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        await repo.AddAsync(warehouse, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
