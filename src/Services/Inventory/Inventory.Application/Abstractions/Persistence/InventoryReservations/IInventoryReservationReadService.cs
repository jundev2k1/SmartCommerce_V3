using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryReservations;

public interface IInventoryReservationReadService
{
    Task<InventoryReservation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryReservation>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventoryReservation?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetActiveByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
