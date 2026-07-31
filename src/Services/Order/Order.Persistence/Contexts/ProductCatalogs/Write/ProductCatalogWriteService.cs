using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Persistence.ProductCatalogs;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using Order.Persistence.Contexts.ProductCatalogs.Repositories;

namespace Order.Persistence.Contexts.ProductCatalogs.Write;

/// <summary>
/// Never calls IUnitOfWork itself - every one of this catalog's 5 event handlers owns a bare
/// unitOfWork.SaveChangesAsync() itself around one or more of these calls (no ExecuteTransactionAsync
/// anywhere in this aggregate's write path today).
/// </summary>
public sealed class ProductCatalogWriteService(
    IRepository<ProductCatalog> repo,
    IProductCatalogRepository catalogRepo) : IProductCatalogWriteService
{
    public async Task CreateAsync(ProductCatalog entry, CancellationToken ct = default)
    {
        await repo.AddAsync(entry, ct);
    }

    public async Task UpdateVariationSnapshotAsync(
        Guid productId,
        Guid variationId,
        string name,
        Sku sku,
        Money price,
        ProductCatalogStatus status,
        CancellationToken ct = default)
    {
        await catalogRepo.UpdateAsync(productId, variationId, entry =>
        {
            entry.UpdateSku(sku);
            entry.UpdatePricing(price);
            entry.UpdateName(name);
            entry.UpdateStatus(status);
        }, ct);
    }

    public async Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default)
    {
        await catalogRepo.DeleteAsync(productId, variationId, ct);
    }

    public async Task DeleteByProductIdAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        await catalogRepo.DeleteProductAsync(productId, ct);
    }

    public async Task UpdateProductNameByProductIdAsync(
        Guid productId,
        string name,
        CancellationToken ct = default)
    {
        await catalogRepo.UpdateProductNameByProductIdAsync(productId, name, ct);
    }
}
