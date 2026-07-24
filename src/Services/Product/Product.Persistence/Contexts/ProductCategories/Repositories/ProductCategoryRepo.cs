using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.ProductCategories.Repositories;

public sealed class ProductCategoryRepo(ProductDbContext dbContext) : IProductCategoryRepository, IRepository<ProductCategory>
{
    public async Task<ProductCategory?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id.Equals(id), ct);
    }

    public async Task<ProductCategory?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<ProductCategory>, IQueryable<ProductCategory>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.ProductCategories);
        return await query.FirstOrDefaultAsync(c => c.Id.Equals(id), ct);
    }

    public async Task AddAsync(ProductCategory entity, CancellationToken ct = default)
    {
        await dbContext.ProductCategories.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<ProductCategory> entities, CancellationToken ct = default)
    {
        await dbContext.ProductCategories.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<ProductCategory, Task> updateAction, CancellationToken ct = default)
    {
        var category = await dbContext.ProductCategories
            .FirstOrDefaultAsync(c => c.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductCategory), id!);

        await updateAction(category);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<ProductCategory>, IQueryable<ProductCategory>> includes,
        Func<ProductCategory, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.ProductCategories);
        var category = await query.FirstOrDefaultAsync(c => c.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductCategory), id!);

        await updateAction(category);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var category = await dbContext.ProductCategories
            .FirstOrDefaultAsync(c => c.Id!.Equals(id), ct);

        if (category is not null)
            dbContext.ProductCategories.Remove(category);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.ProductCategories
            .Where(c => ids.Any(id => id!.Equals(c.Id)))
            .ExecuteDeleteAsync(ct);
    }
}
