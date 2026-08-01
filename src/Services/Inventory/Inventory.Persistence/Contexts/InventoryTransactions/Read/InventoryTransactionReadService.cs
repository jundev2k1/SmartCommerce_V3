using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Read;

public sealed class InventoryTransactionReadService(IInventoryTransactionRepository repo) : IInventoryTransactionReadService
{
    public async Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetHistoryAsync(inventoryId, ct);
    }

    public async Task<PaginatedResult<InventoryTransaction>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }
}
