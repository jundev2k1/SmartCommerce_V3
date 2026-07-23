namespace Inventory.Persistence.Inventories.Repositories;

public interface IInventoryRepository
{
    Task AddAsync(InventoryEntity entity, CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);

    Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default);
}
