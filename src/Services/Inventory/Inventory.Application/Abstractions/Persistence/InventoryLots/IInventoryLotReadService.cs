using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Inventory.Application.Abstractions.Persistence.InventoryLots;

public interface IInventoryLotReadService
{
    Task<InventoryLot?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
