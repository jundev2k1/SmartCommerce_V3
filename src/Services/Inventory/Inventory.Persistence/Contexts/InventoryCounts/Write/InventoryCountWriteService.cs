using Inventory.Application.Abstractions.Persistence.InventoryCounts;
using Inventory.Persistence.Contexts.InventoryCounts.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryCounts.Write;

public sealed class InventoryCountWriteService(
    IInventoryCountRepository repo,
    IUnitOfWork unitOfWork) : IInventoryCountWriteService
{
    public async Task AddAsync(CreateInventoryCountRequest request, CancellationToken ct = default)
    {
        var entity = InventoryCount.Create(
            request.Number,
            request.WarehouseId,
            request.CountDate,
            request.Description);

        await repo.AddAsync(entity, ct);
        await unitOfWork.ExecuteTransactionAsync(ct);
    }
}
