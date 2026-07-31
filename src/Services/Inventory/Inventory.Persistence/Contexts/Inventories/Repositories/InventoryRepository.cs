using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Inventory.Application.Features.Inventories.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Inventories.Repositories;

public sealed class InventoryRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryEntity, Guid>(dbContext), IInventoryRepository
{
    public async Task<PaginatedResult<InventoryEntity>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.Inventories
            .AsNoTracking()
            .ApplyCriteria(InventoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task DeleteByProductIdAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var rows = await _dbContext.Inventories
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);

        _dbContext.Inventories.RemoveRange(rows);
    }

    public async Task DeleteByVariationIdAsync(
        Guid productVariationId,
        CancellationToken ct = default)
    {
        var rows = await _dbContext.Inventories
            .Where(i => i.VariationId == productVariationId)
            .ToListAsync(ct);

        _dbContext.Inventories.RemoveRange(rows);
    }
}
