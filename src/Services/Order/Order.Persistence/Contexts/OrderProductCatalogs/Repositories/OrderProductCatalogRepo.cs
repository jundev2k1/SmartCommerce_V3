using BuildingBlock.Application.Exceptions;

using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.OrderProductCatalogs.Repositories;

public sealed class OrderProductCatalogRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderProductCatalog>(dbContext), IOrderProductCatalogRepository
{
    public async Task UpdateAsync(
        Guid productId,
        Guid variationId,
        Action<OrderProductCatalog> updateAction,
        CancellationToken ct = default)
    {
        var entry = await _dbContext.OrderProductCatalogs
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.VariationId == variationId, ct)
            ?? throw new NotFoundException(nameof(OrderProductCatalog), variationId);

        updateAction?.Invoke(entry);
    }

    public async Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default)
    {
        await _dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId && p.VariationId == variationId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        await _dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> UpdateProductNameByProductIdAsync(
        Guid productId,
        string name,
        CancellationToken ct = default)
    {
        return await _dbContext.OrderProductCatalogs
            .Where(p => p.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.ProductName, name), ct);
    }
}
