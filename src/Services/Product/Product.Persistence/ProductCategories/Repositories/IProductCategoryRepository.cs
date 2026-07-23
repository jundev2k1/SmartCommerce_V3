namespace Product.Persistence.ProductCategories.Repositories;

public interface IProductCategoryRepository
{
    Task AddAsync(ProductCategory entity, CancellationToken ct = default);

    void Remove(ProductCategory entity);
}
