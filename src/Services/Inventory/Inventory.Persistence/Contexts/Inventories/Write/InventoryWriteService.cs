using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Persistence.Contexts.Inventories.Repositories;

namespace Inventory.Persistence.Contexts.Inventories.Write;

/// <summary>
/// Never calls IUnitOfWork itself. AddAsync/DeleteByProductIdAsync/DeleteByVariationIdAsync are
/// each the sole write in their caller's transaction (OnProductVariationCreated/OnProductDeleted/
/// OnProductVariationDeleted own ExecuteTransactionAsync themselves). StageUpdateAsync is used
/// only by the cross-aggregate stock-mutation handlers (AdjustStock/DeductStock/RestockStock/
/// StockIn/StockOut), which own IUnitOfWork directly and batch this with InventoryTransaction/
/// StockDeduction writes - see Correction 2 in the persistence refactor tracker.
/// </summary>
public sealed class InventoryWriteService(
    IRepository<InventoryEntity> repo,
    IInventoryRepository inventoryRepo) : IInventoryWriteService
{
    public async Task AddAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByProductIdAsync(productId, ct);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByVariationIdAsync(productVariationId, ct);
    }

    public async Task StageUpdateAsync(Guid id, Func<InventoryEntity, Task> updateAction, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, updateAction, ct);
    }
}
