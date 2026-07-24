using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Inventory.Application.Abstractions.Persistence.InventoryTransactions;

public interface IInventoryTransactionReadService
{
    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default);

    Task<PaginatedResult<InventoryTransaction>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);
}
