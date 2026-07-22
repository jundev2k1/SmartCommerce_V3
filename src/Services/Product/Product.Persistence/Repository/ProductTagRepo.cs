using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Persistence.Repository;

public sealed class ProductTagRepo(ProductDbContext dbContext) : IProductTagRepository
{
    public async Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddAsync(ProductTag entity, CancellationToken ct = default)
    {
        await dbContext.ProductTags.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<ProductTag, Task> updateAction,
        CancellationToken ct = default)
    {
        var tag = await dbContext.ProductTags
            .FirstOrDefaultAsync(t => t.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductTag), id!);

        await updateAction(tag);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await dbContext.ProductTags
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (tag is not null)
            dbContext.ProductTags.Remove(tag);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // Compare the whole value-converted property, not .Value on it - see ProductRepo.CodeExistsAsync.
        var normalized = TagCode.Create(code);
        return await dbContext.ProductTags.AsNoTracking().AnyAsync(t => t.Code == normalized, ct);
    }

    public async Task<IReadOnlyList<ProductTag>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ProductTags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
    }
}
