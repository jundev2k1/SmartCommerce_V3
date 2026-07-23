namespace Inventory.Persistence.Inventories.Repositories;

public sealed class InventoryRepo(InventoryDbContext dbContext) : IInventoryRepository
{
    public async Task AddAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        await dbContext.Inventories.AddAsync(entity, ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var rows = await dbContext.Inventories
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);

        dbContext.Inventories.RemoveRange(rows);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        var rows = await dbContext.Inventories
            .Where(i => i.ProductVariationId == productVariationId)
            .ToListAsync(ct);

        dbContext.Inventories.RemoveRange(rows);
    }
}
