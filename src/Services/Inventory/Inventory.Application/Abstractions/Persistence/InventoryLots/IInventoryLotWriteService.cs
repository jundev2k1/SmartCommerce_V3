namespace Inventory.Application.Abstractions.Persistence.InventoryLots;

public sealed record CreateInventoryLotRequest(
    Guid InventoryId,
    string LotNumber,
    DateTime ManufactureDate,
    DateTime ExpiredDate,
    int Quantity,
    string SupplierLotNumber = "",
    string CountryOfOrigin = "");

public interface IInventoryLotWriteService
{
    Task AddAsync(CreateInventoryLotRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
