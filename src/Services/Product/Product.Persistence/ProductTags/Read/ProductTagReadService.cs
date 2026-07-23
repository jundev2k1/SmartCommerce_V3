using Product.Application.Abstractions.Persistence.ProductTags;
using Product.Domain.ValueObjects;

namespace Product.Persistence.ProductTags.Read;

public sealed class ProductTagReadService(ProductDbContext dbContext) : IProductTagReadService
{
    public async Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // Compare the whole value-converted property, not .Value on it - see ProductReadService.CodeExistsAsync.
        var normalized = TagCode.Create(code);
        return await dbContext.ProductTags.AsNoTracking().AnyAsync(t => t.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<ProductTag>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ProductTags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
    }
}
