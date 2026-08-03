using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Read;

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
