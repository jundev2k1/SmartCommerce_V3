using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
{
    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryTransaction>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
