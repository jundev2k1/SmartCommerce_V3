using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Persistence.Engine;

namespace Order.Persistence.Contexts.OrderProductCatalogs.Read;

public sealed class OrderProductCatalogReadService(OrderDbContext dbContext) : IOrderProductCatalogReadService
{
    public async Task<OrderProductCatalog[]> GetByVariantionIdsAsync(Guid[] variationIds, CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .AsNoTracking()
            .Where(opc => variationIds.Contains(opc.Id))
            .ToArrayAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.OrderProductCatalogs
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, ct);
    }
}
