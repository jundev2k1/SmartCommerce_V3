using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Persistence.ProductTags;
using Product.Persistence.ProductTags.Repositories;

namespace Product.Persistence.ProductTags.Write;

public sealed class ProductTagWriteService(
    ProductDbContext dbContext,
    IProductTagRepository repo,
    IUnitOfWork unitOfWork) : IProductTagWriteService
{
    public async Task CreateAsync(ProductTag tag, CancellationToken ct = default)
    {
        await repo.AddAsync(tag, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, Func<ProductTag, Task> updateAction, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var tag = await dbContext.ProductTags
                .FirstOrDefaultAsync(t => t.Id == id, ct)
                ?? throw new NotFoundException(nameof(ProductTag), id);

            await updateAction(tag);
        }, ct: ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tag = await dbContext.ProductTags.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is not null)
            repo.Remove(tag);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
