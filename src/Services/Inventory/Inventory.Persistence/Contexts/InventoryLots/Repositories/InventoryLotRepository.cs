using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Criteria;
using SmartEcommerce.Inventory.Application.Features.Inventories.Search;
using SmartEcommerce.Inventory.Application.Features.InventoryLots.Search;
using SmartEcommerce.Inventory.Persistence.Engine;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Repositories;

public sealed class InventoryLotRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryLot, Guid>(dbContext), IInventoryLotRepository
{
    public async Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryLots
            .AsNoTracking()
            .Where(l => l.InventoryId == inventoryId)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryLots
            .AsNoTracking()
            .ApplyCriteria(InventoryLotCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
