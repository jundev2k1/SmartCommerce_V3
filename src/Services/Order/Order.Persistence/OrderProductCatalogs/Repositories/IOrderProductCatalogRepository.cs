namespace Order.Persistence.OrderProductCatalogs.Repositories;

public interface IOrderProductCatalogRepository
{
    Task UpdateProductNameByProductIdAsync(Guid productId, string productName, CancellationToken ct = default);

    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default);
}
