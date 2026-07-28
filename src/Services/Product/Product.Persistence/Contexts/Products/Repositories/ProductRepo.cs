using BuildingBlock.Application.Exceptions;
using BuildingBlock.Persistence.Repository;

using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.Products.Repositories;

public sealed class ProductRepo(ProductDbContext dbContext) : IProductRepository, IRepository<ProductEntity>
{
    // Variations/CategoryMappings/TagMappings are now real (non-owned) relationships, so unlike
    // before they need an explicit Include - callers of this no-includes overload previously got
    // them for free. Always including them here keeps that same "always loaded" behavior for
    // every caller (GetProduct, search projection, ...) without each one repeating the Include.
    public async Task<ProductEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Variations)
            .Include(p => p.CategoryMappings)
            .ThenInclude(cm => cm.Category)
            .Include(p => p.TagMappings)
            .ThenInclude(tm => tm.Tag)
            .FirstOrDefaultAsync(p => p.Id.Equals(id), ct);
    }

    public async Task<ProductEntity?> GetByIdAsync<TId>(
        TId id,
        Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>> includes,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Products);
        return await query.FirstOrDefaultAsync(p => p.Id.Equals(id), ct);
    }

    public async Task<int> GetNextVariationDisplayOrderAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        return await dbContext.ProductVariations
            .AsNoTracking()
            .Where(pv => pv.ProductId == productId)
            .MaxAsync(pv => pv.DisplayOrder, ct) + 1;
    }

    public async Task AddAsync(ProductEntity entity, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<ProductEntity> entities, CancellationToken ct = default)
    {
        await dbContext.Products.AddRangeAsync(entities, ct);
    }

    public async Task AddVariationAsync(ProductVariation variation, CancellationToken ct = default)
    {
        await dbContext.ProductVariations.AddAsync(variation, ct);
    }

    public async Task AddVariationRangeAsync(IEnumerable<ProductVariation> variations, CancellationToken ct = default)
    {
        await dbContext.ProductVariations.AddRangeAsync(variations, ct);
    }

    public async Task UpdateAsync<TId>(TId id, Func<ProductEntity, Task> updateAction, CancellationToken ct = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductEntity), id!);

        await updateAction(product);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>> includes,
        Func<ProductEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.Products);
        var product = await query.FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductEntity), id!);

        await updateAction(product);
    }

    public async Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<ProductVariation>, IQueryable<ProductVariation>> includes,
        Func<ProductVariation, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(dbContext.ProductVariations);
        var variation = await query.FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductVariation), id!);

        await updateAction(variation);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id!.Equals(id), ct);

        if (product is not null)
            dbContext.Products.Remove(product);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        await dbContext.Products
            .Where(p => ids.Any(id => id!.Equals(p.Id)))
            .ExecuteDeleteAsync(ct);
    }

    public async Task RemoveVariationAsync(Guid id, CancellationToken ct = default)
    {
        var variation = await dbContext.ProductVariations
            .FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException(nameof(ProductVariation), id!);

        dbContext.Remove(variation);
    }
}
