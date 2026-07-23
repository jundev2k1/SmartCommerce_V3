using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Inventories.Repositories;

public sealed class InventoryRepo(InventoryDbContext dbContext) : IInventoryRepository, IRepository<InventoryEntity>
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

    public async Task AddAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        await dbContext.Inventories.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<InventoryEntity> entities, CancellationToken ct = default)
    {
        await dbContext.Inventories.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<InventoryEntity, Task> updateAction, CancellationToken ct = default)
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

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.Id!.Equals(id), ct);

        if (inventory is not null)
            dbContext.Inventories.Remove(inventory);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.Inventories
            .Where(i => ids.Any(id => id!.Equals(i.Id)))
            .ExecuteDeleteAsync(ct);
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
