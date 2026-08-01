using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.InventoryDocuments.Repositories;

public interface IInventoryDocumentRepository : IRepository<InventoryDocument, Guid>
{
    Task<InventoryDocument?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetBySourceWarehouseIdAsync(
        Guid sourceWarehouseId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryDocument>> GetByDestinationWarehouseIdAsync(
        Guid destinationWarehouseId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryDocument>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
