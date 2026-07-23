namespace Order.Application.Abstractions.Persistence.OrderProductCatalogs;

public interface IOrderProductCatalogReadService
{
    Task<OrderProductCatalog[]> GetByVariantionIdsAsync(Guid[] variationIds, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
