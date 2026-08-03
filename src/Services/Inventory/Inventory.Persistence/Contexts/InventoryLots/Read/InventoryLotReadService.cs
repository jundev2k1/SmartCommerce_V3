using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryLots;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Repositories;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Read;

public sealed class InventoryLotReadService(IInventoryLotRepository repo) : IInventoryLotReadService
{
    public async Task<InventoryLot?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryLot>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetByInventoryIdAsync(inventoryId, ct);
    }
}
