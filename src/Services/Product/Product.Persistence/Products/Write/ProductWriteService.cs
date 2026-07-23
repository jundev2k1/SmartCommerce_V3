using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Persistence.Products;
using Product.Persistence.Products.Repositories;

namespace Product.Persistence.Products.Write;

public sealed class ProductWriteService(
    ProductDbContext dbContext,
    IProductRepository repo,
    IUnitOfWork unitOfWork) : IProductWriteService
{
    public async Task CreateAsync(ProductEntity product, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(product, ct);
        }, ct: ct);
    }

    public async Task UpdateAsync(Guid id, Func<ProductEntity, Task> updateAction, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await StageUpdateAsync(id, updateAction, ct);
        }, ct: ct);
    }

    public async Task StageUpdateAsync(Guid id, Func<ProductEntity, Task> updateAction, CancellationToken ct = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException(nameof(ProductEntity), id);

        await updateAction(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (product is not null)
                repo.Remove(product);
        }, ct: ct);
    }
}
