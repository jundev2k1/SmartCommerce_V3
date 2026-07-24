using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Warehouses.Repositories;

public sealed class WarehouseRepo(InventoryDbContext dbContext) : IWarehouseRepository, IRepository<Warehouse>
{
    public async Task<Warehouse?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id.Equals(id), ct);
    }

    public async Task<Warehouse?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<Warehouse>, IQueryable<Warehouse>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Warehouses);
        return await query.FirstOrDefaultAsync(w => w.Id.Equals(id), ct);
    }

    public async Task AddAsync(Warehouse entity, CancellationToken ct = default)
    {
        await dbContext.Warehouses.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Warehouse> entities, CancellationToken ct = default)
    {
        await dbContext.Warehouses.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<Warehouse, Task> updateAction, CancellationToken ct = default)
    {
        var warehouse = await dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(warehouse);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<Warehouse>, IQueryable<Warehouse>> includes,
        Func<Warehouse, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Warehouses);
        var warehouse = await query.FirstOrDefaultAsync(w => w.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(warehouse);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var warehouse = await dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id!.Equals(id), ct);

        if (warehouse is not null)
            dbContext.Warehouses.Remove(warehouse);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.Warehouses
            .Where(w => ids.Any(id => id!.Equals(w.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
