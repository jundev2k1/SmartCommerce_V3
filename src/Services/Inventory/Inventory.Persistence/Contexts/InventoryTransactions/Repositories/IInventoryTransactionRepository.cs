using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction, Guid>
{
    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryTransaction>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
