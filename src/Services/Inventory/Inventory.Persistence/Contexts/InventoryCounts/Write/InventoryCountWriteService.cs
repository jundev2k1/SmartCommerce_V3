using BuildingBlock.Application.Abstractions.Persistence;
using Inventory.Application.Abstractions.Persistence.InventoryCounts;
using Inventory.Persistence.Contexts.InventoryCounts.Repositories;

namespace Inventory.Persistence.Contexts.InventoryCounts.Write;

public sealed class InventoryCountWriteService(
    IInventoryCountRepository repo,
    IUnitOfWork unitOfWork) : IInventoryCountWriteService
{
    public async Task AddAsync(InventoryCount entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
