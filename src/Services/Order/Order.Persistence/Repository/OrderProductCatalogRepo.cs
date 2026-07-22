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
}
