using SmartEcommerce.BuildingBlock.Domain.ValueObjects;

namespace SmartEcommerce.Product.Domain.Entities.Products.Data;

public sealed record CreateBundleItemData(
    Guid BundleVariantId,
    Guid ProductId,
    Guid VariantId,
    Quantity Quantity,
    int DisplayOrder = 0);
