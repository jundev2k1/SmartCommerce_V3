namespace Product.Domain.Entities;

/// <summary>
/// Explicit many-to-many join entity between Product and ProductCategory - Product and
/// ProductCategory are independent aggregate roots, so this row (not a raw id collection) is
/// how Product references a category without holding an object reference to another root.
/// Owned by Product (no independent identity, no repository of its own).
/// </summary>
public sealed class ProductCategoryMapping : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }

    private ProductCategoryMapping() { }

    internal static ProductCategoryMapping Create(Guid id, Guid productId, Guid categoryId)
    {
        return new ProductCategoryMapping
        {
            Id = id,
            ProductId = productId,
            CategoryId = categoryId,
        };
    }
}
