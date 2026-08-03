using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventorySerials;

public interface IInventorySerialReadService
{
    Task<InventorySerial?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventorySerial>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventorySerial?> GetBySerialNumberAsync(
        string serialNumber,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetAvailableByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
