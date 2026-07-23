namespace Product.Application.Abstractions.Persistence.ProductCategories;

public interface IProductCategoryWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateProductCategoryHandler's current shape.</summary>
    Task CreateAsync(ProductCategory category, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Func<ProductCategory, Task> updateAction, CancellationToken ct = default);

    /// <summary>Commits via bare SaveChangesAsync, matching DeleteProductCategoryHandler's current shape.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
