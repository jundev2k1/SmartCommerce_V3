namespace Product.Persistence.ProductTags.Repositories;

public interface IProductTagRepository
{
    Task AddAsync(ProductTag entity, CancellationToken ct = default);

    void Remove(ProductTag entity);
}
