namespace Product.Persistence.ProductTags.Repositories;

public sealed class ProductTagRepo(ProductDbContext dbContext) : IProductTagRepository
{
    public async Task AddAsync(ProductTag entity, CancellationToken ct = default)
    {
        await dbContext.ProductTags.AddAsync(entity, ct);
    }

    public void Remove(ProductTag entity)
    {
        dbContext.ProductTags.Remove(entity);
    }
}
