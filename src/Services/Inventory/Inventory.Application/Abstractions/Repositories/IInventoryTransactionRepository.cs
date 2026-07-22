namespace Inventory.Application.Abstractions.Repositories;

public interface IInventoryTransactionRepository
{
    Task AddAsync(InventoryTransaction entity, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryTransaction>> GetHistoryAsync(Guid inventoryId, CancellationToken ct = default);
}
