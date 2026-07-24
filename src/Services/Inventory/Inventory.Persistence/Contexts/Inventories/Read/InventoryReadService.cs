using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Features.Inventories.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Inventories.Read;

public sealed class InventoryReadService(InventoryDbContext dbContext) : IInventoryReadService
{
    public async Task<InventoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<PaginatedResult<InventoryEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .ApplyCriteria(InventoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task<InventoryEntity?> GetByVariationAndWarehouseAsync(
        Guid productVariationId,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductVariationId == productVariationId && i.WarehouseId == warehouseId, ct);
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<int> GetTotalStockByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.ProductVariationId == productVariationId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => productVariationIds.Contains(i.ProductVariationId))
            .GroupBy(i => i.ProductVariationId)
            .Select(g => new { VariationId = g.Key, Total = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.VariationId, x => x.Total, ct);
    }
}
