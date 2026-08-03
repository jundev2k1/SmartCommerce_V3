using SmartEcommerce.Product.Domain.Entities.Tags;

namespace SmartEcommerce.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product,
/// referenced back via Product.TagMappings - the many-to-many mapping of products to tags.
/// </summary>
public sealed class ProductTagMapping : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid TagId { get; private set; }
    public ProductTag Tag { get; private set; } = default!;
    public int DisplayOrder { get; private set; }

    private ProductTagMapping() { }

    internal static ProductTagMapping Create(Guid productId, Guid tagId, int displayOrder = 0)
    {
        return new ProductTagMapping
        {
            ProductId = productId,
            TagId = tagId,
            DisplayOrder = displayOrder,
        };
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
