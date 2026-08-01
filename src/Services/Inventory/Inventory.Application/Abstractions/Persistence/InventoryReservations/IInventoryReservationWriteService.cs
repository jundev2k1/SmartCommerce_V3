namespace Inventory.Application.Abstractions.Persistence.InventoryReservations;

public sealed record CreateInventoryReservationRequest(
    string Number,
    InventoryReservationType Type,
    Guid InventoryId,
    Guid WarehouseId,
    Guid ProductId,
    Guid ProductVariantId,
    int Quantity,
    InventoryReferenceType? ReferenceType = null,
    Guid? ReferenceId = null,
    string ExternalReference = "",
    DateTime? ExpiredAt = null,
    string Reason = "");

public interface IInventoryReservationWriteService
{
    Task AddAsync(CreateInventoryReservationRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
