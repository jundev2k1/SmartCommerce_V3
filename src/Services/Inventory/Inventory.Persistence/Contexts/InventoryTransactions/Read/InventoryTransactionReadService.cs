using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Features.Inventories.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Read;

public sealed class InventoryTransactionReadService(InventoryDbContext dbContext) : IInventoryTransactionReadService
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await dbContext.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryTransaction>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.InventoryTransactions
            .AsNoTracking()
            .ApplyCriteria(InventoryTransactionCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
