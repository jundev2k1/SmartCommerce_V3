namespace Product.Persistence.ProductCategories.Repositories;

public sealed class ProductCategoryRepo(ProductDbContext dbContext) : IProductCategoryRepository
{
    public async Task AddAsync(ProductCategory entity, CancellationToken ct = default)
    {
        await dbContext.ProductCategories.AddAsync(entity, ct);
    }

    public void Remove(ProductCategory entity)
    {
        dbContext.ProductCategories.Remove(entity);
    }
}
