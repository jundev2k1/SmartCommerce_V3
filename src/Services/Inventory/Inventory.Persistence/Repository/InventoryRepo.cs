using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Persistence.Repository;

public sealed class InventoryRepo(InventoryDbContext dbContext) : IInventoryRepository
{
    public async Task<InventoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<InventoryEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<InventoryEntity>, IQueryable<InventoryEntity>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Inventories);
        return await query.FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<InventoryEntity?> GetByVariationAndWarehouseAsync(
        Guid productVariationId,
        Guid warehouseId,
        CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductVariationId == productVariationId && i.WarehouseId == warehouseId, ct);
    }

    public async Task AddAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        await dbContext.Inventories.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<InventoryEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(InventoryEntity), id!);

        await updateAction(inventory);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<InventoryEntity>, IQueryable<InventoryEntity>> includes,
        Func<InventoryEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Inventories);
        var inventory = await query.FirstOrDefaultAsync(i => i.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(InventoryEntity), id!);

        await updateAction(inventory);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (inventory is not null)
            dbContext.Inventories.Remove(inventory);
    }

    public async Task<IReadOnlyList<InventoryEntity>> GetByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .Where(i => i.ProductVariationId == productVariationId)
            .ToListAsync(ct);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        var rows = await dbContext.Inventories
            .Where(i => i.ProductVariationId == productVariationId)
            .ToListAsync(ct);

        dbContext.Inventories.RemoveRange(rows);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var rows = await dbContext.Inventories
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);

        dbContext.Inventories.RemoveRange(rows);
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<int> GetTotalStockByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.ProductVariationId == productVariationId)
            .SumAsync(i => i.Quantity, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default)
    {
        return await dbContext.Inventories
            .AsNoTracking()
            .Where(i => productVariationIds.Contains(i.ProductVariationId))
            .GroupBy(i => i.ProductVariationId)
            .Select(g => new { VariationId = g.Key, Total = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.VariationId, x => x.Total, ct);
    }
}
