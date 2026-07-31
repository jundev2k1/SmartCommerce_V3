using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Persistence.Contexts.Inventories.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Inventories.Read;

public sealed class InventoryReadService(
    IInventoryRepository inventoryRepo,
    InventoryDbContext dbContext) : IInventoryReadService
{
    public async Task<InventoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await inventoryRepo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await inventoryRepo.SearchAsync(request, ct);
    }

    public async Task<InventoryEntity?> GetByVariationAndWarehouseAsync(
        Guid variationId,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        return await inventoryRepo.GetAsync(i =>
            i.VariationId == variationId
            && i.WarehouseId == warehouseId, ct);
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<int> GetTotalStockByVariationIdAsync(Guid variationId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.VariationId == variationId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> variationIds, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => variationIds.Contains(i.VariationId))
            .GroupBy(i => i.VariationId)
            .Select(g => new { VariationId = g.Key, Total = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.VariationId, x => x.Total, ct);
    }
}
