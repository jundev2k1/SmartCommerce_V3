using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.ProductTags.Repositories;

public sealed class ProductTagRepo(ProductDbContext dbContext) : IProductTagRepository, IRepository<ProductTag>
{
    public async Task<ProductTag?> GetByIdAsync<TId>(TId id, Func<IQueryable<ProductTag>, IQueryable<ProductTag>> includes, CancellationToken ct = default)
    {
        var query = dbContext.ProductTags
            .AsNoTracking()
            .AsQueryable();
        query = includes(query);
        return await query.FirstOrDefaultAsync(q => q.Id.Equals(id), ct);
    }

    public async Task<ProductTag?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.Id.Equals(id), ct);
    }

    public async Task AddAsync(ProductTag entity, CancellationToken ct = default)
    {
        await dbContext.ProductTags.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<ProductTag> tags, CancellationToken ct = default)
    {
        await dbContext.ProductTags.AddRangeAsync(tags, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<ProductTag, Task> updateAction, CancellationToken ct = default)
    {
        var target = await dbContext.ProductTags
            .FirstOrDefaultAsync(pt => pt.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(target);
    }
    public async Task UpdateAsync<TId>(TId id, Func<IQueryable<ProductTag>, IQueryable<ProductTag>> includes, Func<ProductTag, Task> updateAction, CancellationToken ct = default)
    {
        var query = dbContext.ProductTags
            .AsQueryable();
        query = includes(query);
        var target = await query
            .FirstOrDefaultAsync(pt => pt.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        await updateAction(target);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var target = await dbContext.ProductTags
            .FirstOrDefaultAsync(pt => pt.Id.Equals(id), ct)
            ?? throw new NotFoundException(nameof(id), id!);

        dbContext.Remove(target);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.ProductTags
            .Where(pt => ids.Any(id => id!.Equals(pt.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
