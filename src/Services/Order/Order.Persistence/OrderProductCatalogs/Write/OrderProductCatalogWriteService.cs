using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Persistence.OrderProductCatalogs.Repositories;

namespace Order.Persistence.OrderProductCatalogs.Write;

/// <summary>
/// Never calls IUnitOfWork itself - every one of this catalog's 5 event handlers owns a bare
/// unitOfWork.SaveChangesAsync() itself around one or more of these calls (no ExecuteTransactionAsync
/// anywhere in this aggregate's write path today).
/// </summary>
public sealed class OrderProductCatalogWriteService(
    IRepository<OrderProductCatalog> repo,
    IOrderProductCatalogRepository catalogRepo) : IOrderProductCatalogWriteService
{
    public async Task CreateAsync(OrderProductCatalog entry, CancellationToken ct = default)
    {
        await repo.AddAsync(entry, ct);
    }

    public async Task UpdateVariationSnapshotAsync(
        Guid id, string productName, string sku, decimal price, string status, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async entry =>
        {
            entry.UpdatePricing(sku, price, status);
            entry.UpdateProductName(productName);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task UpdatePricingAsync(Guid id, string sku, decimal price, string status, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, async entry =>
        {
            entry.UpdatePricing(sku, price, status);
            await Task.CompletedTask;
        }, ct);
    }

    public async Task UpdateProductNameByProductIdAsync(Guid productId, string productName, CancellationToken ct = default)
    {
        await catalogRepo.UpdateProductNameByProductIdAsync(productId, productName, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(id, ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        await catalogRepo.DeleteByProductIdAsync(productId, ct);
    }
}
