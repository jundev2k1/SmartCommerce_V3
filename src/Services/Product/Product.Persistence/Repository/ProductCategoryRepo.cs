using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Persistence.Repository;

public sealed class ProductCategoryRepo(ProductDbContext dbContext) : IProductCategoryRepository
{
    public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(ProductCategory entity, CancellationToken ct = default)
    {
        await dbContext.ProductCategories.AddAsync(entity, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<ProductCategory, Task> updateAction,
        CancellationToken ct = default)
    {
        var category = await dbContext.ProductCategories
            .FirstOrDefaultAsync(c => c.Id!.Equals(id), ct)
            ?? throw new NotFoundException(nameof(ProductCategory), id!);

        await updateAction(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await dbContext.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is not null)
            dbContext.ProductCategories.Remove(category);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // Compare the whole value-converted property, not .Value on it - see ProductRepo.CodeExistsAsync.
        var normalized = CategoryCode.Create(code);
        return await dbContext.ProductCategories.AsNoTracking().AnyAsync(c => c.Code == normalized, ct);
    }

    public async Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories.AsNoTracking().AnyAsync(c => c.ParentCategoryId == categoryId, ct);
    }

    public async Task<IReadOnlyList<Guid>> GetChildIdsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }
}
