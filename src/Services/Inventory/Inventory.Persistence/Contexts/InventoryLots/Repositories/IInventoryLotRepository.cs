using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.InventoryLots.Repositories;

public interface IInventoryLotRepository : IRepository<InventoryLot, Guid>
{
    Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
