using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Repository;

using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Persistence.Contexts.ProductTags.Write;

public sealed class ProductTagWriteService(
    IRepository<ProductTag> repo,
    IUnitOfWork unitOfWork) : IProductTagWriteService
{
    public async Task CreateAsync(ProductTag tag, CancellationToken ct = default)
    {
        await repo.AddAsync(tag, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateTagNameAsync(Guid id, string tagName, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async productTag =>
        {
            productTag.Rename(tagName);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
