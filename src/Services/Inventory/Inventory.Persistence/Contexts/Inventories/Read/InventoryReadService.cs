using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Persistence.Contexts.Inventories.Repositories;

namespace Inventory.Persistence.Contexts.Inventories.Read;

public sealed class InventoryReadService(IInventoryRepository inventoryRepo) : IInventoryReadService
{
    public async Task<InventoryStock?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await inventoryRepo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryStock>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await inventoryRepo.SearchAsync(request, ct);
    }

    public async Task<InventoryStock?> GetByVariationAndWarehouseAsync(
        Guid variationId,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        return await inventoryRepo.GetAsync(i =>
            i.VariantId == variationId
            && i.WarehouseId == warehouseId, ct);
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await inventoryRepo.GetTotalStockByProductIdAsync(productId, ct);
    }

    public async Task<int> GetTotalStockByVariationIdAsync(Guid variationId, CancellationToken ct = default)
    {
        return await inventoryRepo.GetTotalStockByVariationIdAsync(variationId, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> variationIds, CancellationToken ct = default)
    {
        return await inventoryRepo.GetTotalStockByVariationIdsAsync(variationIds, ct);
    }
}
