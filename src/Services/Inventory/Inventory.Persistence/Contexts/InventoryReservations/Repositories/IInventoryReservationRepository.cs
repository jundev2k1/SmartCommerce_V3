using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventoryReservations.Repositories;

public interface IInventoryReservationRepository : IRepository<InventoryReservation, Guid>
{
    Task<InventoryReservation?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetActiveByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<PaginatedResult<InventoryReservation>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
