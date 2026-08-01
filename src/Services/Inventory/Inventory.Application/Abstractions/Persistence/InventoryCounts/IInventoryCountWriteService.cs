namespace Inventory.Application.Abstractions.Persistence.InventoryCounts;

public sealed record CreateInventoryCountRequest(
    string Number,
    Guid WarehouseId,
    DateTime CountDate,
    string Description = "");

public interface IInventoryCountWriteService
{
    Task AddAsync(CreateInventoryCountRequest request, CancellationToken ct = default);
}
