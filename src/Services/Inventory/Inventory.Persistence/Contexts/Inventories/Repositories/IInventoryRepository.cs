using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.Inventories.Repositories;

public interface IInventoryRepository : IRepository<InventoryEntity, Guid>
{
    Task<PaginatedResult<InventoryEntity>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);
}
