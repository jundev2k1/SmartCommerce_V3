namespace Product.Application.Abstractions.Persistence.ProductTags;

public interface IProductTagWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateProductTagHandler's current shape.</summary>
    Task CreateAsync(ProductTag tag, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Func<ProductTag, Task> updateAction, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync, matching DeleteProductTagHandler's current shape.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
