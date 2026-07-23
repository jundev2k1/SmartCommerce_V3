namespace Product.Persistence.Products.Repositories;

public interface IProductRepository
{
    Task AddAsync(ProductEntity entity, CancellationToken ct = default);

    void Remove(ProductEntity entity);
}
