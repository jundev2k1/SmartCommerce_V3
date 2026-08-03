using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Criteria;
using SmartEcommerce.Inventory.Application.Features.Inventories.Search;
using SmartEcommerce.Inventory.Persistence.Engine;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public sealed class InventoryTransactionRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryTransaction, Guid>(dbContext), IInventoryTransactionRepository
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryTransaction>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryTransactions
            .AsNoTracking()
            .ApplyCriteria(InventoryTransactionCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
