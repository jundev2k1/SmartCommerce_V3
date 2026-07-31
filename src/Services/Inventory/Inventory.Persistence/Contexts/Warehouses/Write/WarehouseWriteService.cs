using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.Warehouses;

namespace Inventory.Persistence.Contexts.Warehouses.Write;

public sealed class WarehouseWriteService(
    IRepository<Warehouse> repo,
    IUnitOfWork unitOfWork) : IWarehouseWriteService
{
    public async Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = Warehouse.Create(
            Guid.CreateVersion7(),
            request.Code,
            request.Name,
            request.Address);

        await repo.AddAsync(warehouse, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return warehouse.Id;
    }
}
