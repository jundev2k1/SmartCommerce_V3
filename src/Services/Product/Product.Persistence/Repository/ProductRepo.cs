using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Persistence.Repository;

public sealed class ProductRepo(ProductDbContext dbContext) : IProductRepository
{
    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(ProductEntity entity, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<ProductEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductEntity), id!);

        await updateAction(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is not null)
            dbContext.Products.Remove(product);
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

    public async Task<bool> ExistsWithTagAsync(Guid tagId, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.TagMappings.Any(m => m.TagId == tagId), ct);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetAllAsync(int skip, int take, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
