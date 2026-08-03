using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Product.Persistence.Engine;

namespace SmartEcommerce.Product.Persistence.Contexts.Products.Repositories;

public sealed class ProductRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductEntity>(dbContext), IProductRepository
{
    public async Task<ProductEntity[]> GetAllAsync(
        int skip,
        int take,
        CancellationToken ct = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Variations)
            .Include(p => p.CategoryMappings)
            .ThenInclude(cm => cm.Category)
            .Include(p => p.TagMappings)
            .ThenInclude(tm => tm.Tag)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(ct);
    }

    public async Task<Guid[]> GetProductsByTagIdAsync(
        Guid tagId,
        CancellationToken ct = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.TagMappings.Any(tm => tm.TagId == tagId))
            .Select(p => p.Id)
            .ToArrayAsync(ct);
    }

    public async Task<int> GetNextVariationDisplayOrderAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        return await _dbContext.ProductVariations
            .AsNoTracking()
            .Where(pv => pv.ProductId == productId)
            .MaxAsync(pv => pv.DisplayOrder, ct) + 1;
    }

    public async Task AddVariationAsync(
        ProductVariation variation,
        CancellationToken ct = default)
    {
        await _dbContext.ProductVariations.AddAsync(variation, ct);
    }

    public async Task AddVariationRangeAsync(
        IEnumerable<ProductVariation> variations,
        CancellationToken ct = default)
    {
        await _dbContext.ProductVariations.AddRangeAsync(variations, ct);
    }

    public async Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<ProductVariation>, IQueryable<ProductVariation>> includes,
        Func<ProductVariation, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(_dbContext.ProductVariations);
        var variation = await query.FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductVariation), id!);

        await updateAction(variation);
    }

    public async Task RemoveVariationAsync(Guid id, CancellationToken ct = default)
    {
        var variation = await _dbContext.ProductVariations
            .FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException(nameof(ProductVariation), id!);

        _dbContext.Remove(variation);
    }
}
