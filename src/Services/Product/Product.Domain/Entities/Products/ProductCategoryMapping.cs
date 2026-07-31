using Product.Domain.Entities.Categories;

namespace Product.Domain.Entities.Products;

/// <summary>
/// Explicit many-to-many join entity between Product and ProductCategory - Product and
/// ProductCategory are independent aggregate roots, so this row (not a raw id collection) is
/// how Product references a category without holding an object reference to another root.
/// Owned by Product (no independent identity, no repository of its own).
/// </summary>
public sealed class ProductCategoryMapping : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid CategoryId { get; private set; }
    public ProductCategory Category { get; private set; } = default!;

    private ProductCategoryMapping() { }

    internal static ProductCategoryMapping Create(Guid productId, Guid categoryId)
    {
        return new ProductCategoryMapping
        {
            ProductId = productId,
            CategoryId = categoryId,
        };
    }
}
