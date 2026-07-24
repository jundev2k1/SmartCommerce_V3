using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace Inventory.Application.Abstractions.Persistence.Inventories;

public interface IInventoryReadService
{
    Task<InventoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryEntity>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);

    /// <summary>Stock is keyed at (ProductVariationId, WarehouseId) - see InventoryEntity remarks.</summary>
    Task<InventoryEntity?> GetByVariationAndWarehouseAsync(
        Guid productVariationId,
        Guid warehouseId,
        CancellationToken ct = default);

    /// <summary>Rollup across every variation and warehouse for the whole product.</summary>
    Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Rollup across every warehouse for a single variation.</summary>
    Task<int> GetTotalStockByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);

    /// <summary>Batched rollup across every warehouse for each requested variation, in one query. Variation ids with no inventory rows are absent from the result (callers should default to 0).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default);
}
