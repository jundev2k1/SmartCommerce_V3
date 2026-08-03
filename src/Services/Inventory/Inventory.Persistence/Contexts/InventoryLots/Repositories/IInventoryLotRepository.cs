using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Repositories;

public interface IInventoryLotRepository : IRepository<InventoryLot, Guid>
{
    Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
