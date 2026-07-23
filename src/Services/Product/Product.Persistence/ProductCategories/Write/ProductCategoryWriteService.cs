using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Persistence.ProductCategories;
using Product.Persistence.ProductCategories.Repositories;

namespace Product.Persistence.ProductCategories.Write;

public sealed class ProductCategoryWriteService(
    ProductDbContext dbContext,
    IProductCategoryRepository repo,
    IUnitOfWork unitOfWork) : IProductCategoryWriteService
{
    public async Task CreateAsync(ProductCategory category, CancellationToken ct = default)
    {
        await repo.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, Func<ProductCategory, Task> updateAction, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var category = await dbContext.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), id);

            await updateAction(category);
        }, ct: ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await dbContext.ProductCategories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is not null)
            repo.Remove(category);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
