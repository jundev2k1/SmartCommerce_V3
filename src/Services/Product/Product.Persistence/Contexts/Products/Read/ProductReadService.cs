using BuildingBlock.Persistence.Repository;

using Product.Application.Abstractions.Persistence.Products;
using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.Products.Read;

public sealed class ProductReadService(
    ProductDbContext dbContext,
    IRepository<ProductEntity> repo) : IProductReadService
{
    public async Task<IReadOnlyList<ProductEntity>> GetAllAsync(int skip, int take, CancellationToken ct = default)
    {
        // Same "always loaded" reasoning as ProductRepo.GetByIdAsync - RebuildProductSearchIndexHandler
        // consumes this batch via ProductSearchProjectionBuilder, which needs these collections.
        return await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Variations)
            .Include(p => p.CategoryMappings)
            .Include(p => p.TagMappings)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<Guid[]> GetProductsByTagIdAsync(Guid tagId, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.TagMappings.Any(tm => tm.TagId == tagId))
            .Select(p => p.Id)
            .ToArrayAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // Compare the whole value-converted property, not .Value on it - EF Core can translate
        // "p.Code == someProductCode" (the converter applies to the constant), but not
        // "p.Code.Value == someString" (member access on the post-conversion CLR type).
        var normalized = ProductCode.Create(code);
        return await dbContext.Products.AsNoTracking().AnyAsync(p => p.Code == normalized, ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken ct = default)
    {
        var normalized = Slug.Create(slug);
        return await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Slug == normalized && (excludeProductId == null || p.Id != excludeProductId), ct);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeVariationId = null, CancellationToken ct = default)
    {
        var normalized = Sku.Create(sku);
        return await dbContext.Products
            .AsNoTracking()
            .SelectMany(p => p.Variations)
            .AnyAsync(v => v.Sku == normalized && (excludeVariationId == null || v.Id != excludeVariationId), ct);
    }

    public async Task<string?> GetProductNameBySkuAsync(string sku, CancellationToken ct = default)
    {
        var normalized = Sku.Create(sku);
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Variations.Any(v => v.Sku == normalized))
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsWithCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.CategoryMappings.Any(m => m.CategoryId == categoryId), ct);
    }
}
