using BuildingBlock.Persistence.Repository;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using Order.Persistence.Contexts.OrderProductCatalogs.Repositories;

namespace Order.Persistence.Contexts.OrderProductCatalogs.Write;

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
        Guid productId,
        Guid variationId,
        string name,
        Sku sku,
        Money price,
        OrderProductCatalogStatus status,
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
}
