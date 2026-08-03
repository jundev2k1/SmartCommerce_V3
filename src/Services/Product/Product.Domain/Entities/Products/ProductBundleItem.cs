using SmartEcommerce.BuildingBlock.Domain.ValueObjects;

namespace SmartEcommerce.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product - one child Variant and its quantity that make up a Bundle-type Variant.
/// </summary>
public sealed class ProductBundleItem : BaseEntity<Guid>, IAuditable
{
    public ProductVariant BundleVariant { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public Guid VariantId { get; private set; }
    public ProductVariant Variant { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;

    private ProductBundleItem() { }

    internal static ProductBundleItem Create(
        Guid bundleProductId,
        Guid productId,
        Guid variantId,
        Quantity quantity)
    {
        return new ProductBundleItem
        {
            Id = bundleProductId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
        };
    }
}
