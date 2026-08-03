using SmartEcommerce.Product.Domain.Entities.Tags;

namespace SmartEcommerce.Product.Domain.Entities.Products;

/// <summary>
/// Explicit many-to-many join entity between Product and ProductTag - see
/// ProductCategoryMapping remarks for why this exists instead of a raw id collection.
/// </summary>
public sealed class ProductTagMapping : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid TagId { get; private set; }
    public ProductTag Tag { get; private set; } = default!;

    private ProductTagMapping() { }

    internal static ProductTagMapping Create(Guid productId, Guid tagId)
    {
        return new ProductTagMapping
        {
            ProductId = productId,
            TagId = tagId,
        };
    }
}
