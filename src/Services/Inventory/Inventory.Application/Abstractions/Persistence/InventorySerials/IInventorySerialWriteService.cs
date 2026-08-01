namespace Inventory.Application.Abstractions.Persistence.InventorySerials;

public sealed record CreateInventorySerialRequest(
    Guid InventoryId,
    string SerialNumber);

public interface IInventorySerialWriteService
{
    Task AddAsync(CreateInventorySerialRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
