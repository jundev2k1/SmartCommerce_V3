using BuildingBlock.Persistence.Repository;

namespace Order.Persistence.Contexts.OrderProductCatalogs.Repositories;

public interface IOrderProductCatalogRepository : IRepository<OrderProductCatalog>
{
    Task UpdateAsync(
        Guid productId,
        Guid variationId,
        Action<OrderProductCatalog> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default);

    Task DeleteProductAsync(
        Guid productId,
        CancellationToken ct = default);
}
