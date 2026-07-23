namespace Product.Persistence.Products.Repositories;

public sealed class ProductRepo(ProductDbContext dbContext) : IProductRepository
{
    public async Task AddAsync(ProductEntity entity, CancellationToken ct = default)
    {
        await dbContext.Products.AddAsync(entity, ct);
    }

    public void Remove(ProductEntity entity)
    {
        dbContext.Products.Remove(entity);
    }
}
