using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

namespace Order.Persistence.OrderProductCatalogs.Repositories;

public sealed class OrderProductCatalogRepo(OrderDbContext dbContext)
    : IOrderProductCatalogRepository, IRepository<OrderProductCatalog>
{
    public async Task<OrderProductCatalog?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id.Equals(id), ct);
    }

    public async Task<OrderProductCatalog?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.OrderProductCatalogs);
        return await query.FirstOrDefaultAsync(p => p.Id.Equals(id), ct);
    }

    public async Task AddAsync(OrderProductCatalog entity, CancellationToken ct = default)
    {
        await dbContext.OrderProductCatalogs.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<OrderProductCatalog> entities, CancellationToken ct = default)
    {
        await dbContext.OrderProductCatalogs.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<OrderProductCatalog, Task> updateAction, CancellationToken ct = default)
    {
        var entry = await dbContext.OrderProductCatalogs
            .FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(OrderProductCatalog), id!);

        await updateAction(entry);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<OrderProductCatalog>, IQueryable<OrderProductCatalog>> includes,
        Func<OrderProductCatalog, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.OrderProductCatalogs);
        var entry = await query.FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(OrderProductCatalog), id!);

        await updateAction(entry);
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

    public async Task UpdateProductNameByProductIdAsync(Guid productId, string productName, CancellationToken ct = default)
    {
        var rows = await dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ToArrayAsync(ct);

        foreach (var row in rows)
            row.UpdateProductName(productName);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        var rows = await dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ToArrayAsync(ct);

        dbContext.OrderProductCatalogs.RemoveRange(rows);
    }
}
