using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Persistence.Contexts.Inventories.Repositories;

namespace Inventory.Persistence.Contexts.Inventories.Write;

/// <summary>
/// Never calls IUnitOfWork itself. AddAsync/DeleteByProductIdAsync/DeleteByVariationIdAsync are
/// each the sole write in their caller's transaction (OnProductVariationCreated/OnProductDeleted/
/// OnProductVariationDeleted own ExecuteTransactionAsync themselves). IncreaseAsync/DecreaseAsync/
/// AdjustToAsync are used only by the cross-aggregate stock-mutation handlers (AdjustStock/
/// DeductStock/RestockStock/StockIn/StockOut), which own IUnitOfWork directly and batch this with
/// InventoryTransaction/StockDeduction writes - see Correction 2 in the persistence refactor tracker.
/// </summary>
public sealed class InventoryWriteService(
    IInventoryRepository inventoryRepo) : IInventoryWriteService
{
    public async Task AddAsync(CreateInventoryRequest request, CancellationToken ct = default)
    {
        var entity = InventoryEntity.Create(
            Guid.CreateVersion7(),
            request.ProductId,
            request.VariationId,
            request.WarehouseId,
            request.Quantity);

        await inventoryRepo.AddAsync(entity, ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByProductIdAsync(productId, ct);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByVariationIdAsync(productVariationId, ct);
    }

    public async Task<InventoryEntity> IncreaseAsync(Guid id, int amount, CancellationToken ct = default)
    {
        InventoryEntity? entity = null;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            inv.Increase(amount);
            entity = inv;
        }, ct);

        return entity!;
    }

    public async Task<InventoryEntity> DecreaseAsync(Guid id, int amount, CancellationToken ct = default)
    {
        InventoryEntity? entity = null;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            inv.Decrease(amount);
            entity = inv;
        }, ct);

        return entity!;
    }

    public async Task<InventoryAdjustmentResult> AdjustToAsync(Guid id, int newQuantity, CancellationToken ct = default)
    {
        InventoryEntity? entity = null;
        var delta = 0;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            delta = newQuantity - inv.Available;
            inv.Adjust(newQuantity);
            entity = inv;
        }, ct);

        return new InventoryAdjustmentResult(entity!, delta);
    }
}
