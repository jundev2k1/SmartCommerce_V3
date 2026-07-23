using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Repositories;

namespace Order.Persistence.Repository;

public sealed class OrderProductCatalogRepo(OrderDbContext dbContext)
    : IOrderProductCatalogRepository, IRepository<OrderProductCatalog>
{
    public async Task<OrderProductCatalog?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<OrderProductCatalog?> GetByIdAsync(
        Guid id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.OrderProductCatalogs);
        return await query.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<OrderProductCatalog[]> GetByVariantionIdsAsync(
        Guid[] variationIds,
        CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .AsNoTracking()
            .Where(opc => variationIds.Contains(opc.Id))
            .ToArrayAsync(ct);
    }

    public async Task AddAsync(OrderProductCatalog entity, CancellationToken ct = default)
    {
        await dbContext.OrderProductCatalogs.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<OrderProductCatalog, Task> updateAction,
        CancellationToken ct = default)
    {
        var entry = await dbContext.OrderProductCatalogs
            .FirstOrDefaultAsync(p => p.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(entry);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        Func<OrderProductCatalog, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.OrderProductCatalogs);
        var entry = await query.FirstOrDefaultAsync(p => p.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(entry);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await dbContext.OrderProductCatalogs
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entry is not null)
            dbContext.OrderProductCatalogs.Remove(entry);
    }

    public async Task<IReadOnlyList<OrderProductCatalog>> GetByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ToListAsync(ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var rows = await dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ToListAsync(ct);

        dbContext.OrderProductCatalogs.RemoveRange(rows);
    }

    // Generic IRepository<T> shape (extended alongside AddRangeAsync/DeleteRangeAsync below) -
    // kept as separate overloads from the non-generic DeleteAsync above, which
    // IOrderProductCatalogRepository's own contract still expects; not yet consumed here, added
    // only to satisfy the interface ahead of Order's own persistence-refactor phase (see
    // docs/refactoring/persistence-refactor-plan.md).
    public async Task AddRangeAsync(IEnumerable<OrderProductCatalog> entities, CancellationToken ct = default)
    {
        await dbContext.OrderProductCatalogs.AddRangeAsync(entities, ct);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var entry = await dbContext.OrderProductCatalogs
            .FirstOrDefaultAsync(p => p.Id!.Equals(id), ct);

        if (entry is not null)
            dbContext.OrderProductCatalogs.Remove(entry);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.OrderProductCatalogs
            .Where(p => ids.Any(id => id!.Equals(p.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
