namespace Inventory.Application.Abstractions.Persistence.InventoryCounts;

public interface IInventoryCountWriteService
{
    Task AddAsync(InventoryCount entity, CancellationToken ct = default);
}
