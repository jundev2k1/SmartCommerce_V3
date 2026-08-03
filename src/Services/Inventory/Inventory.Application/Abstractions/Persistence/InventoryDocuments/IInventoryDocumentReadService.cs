using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryDocuments;

public interface IInventoryDocumentReadService
{
    Task<InventoryDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryDocument>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventoryDocument?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetBySourceWarehouseIdAsync(
        Guid sourceWarehouseId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetByDestinationWarehouseIdAsync(
        Guid destinationWarehouseId,
        CancellationToken ct = default);
}
