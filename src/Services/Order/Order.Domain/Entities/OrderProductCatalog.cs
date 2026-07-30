using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

/// <summary>
/// Local read-model of variation name/sku/price, kept in sync via the Product/ProductVariation
/// integration events so Order can price/validate requested variations without a synchronous
/// call to Product Service. Id is the ProductVariationId itself (1:1 with a variation, no
/// surrogate key) - keyed at variation level, not product level, since that's the actual
/// priced/orderable unit now that a Product can have many variations.
/// </summary>
public sealed class OrderProductCatalog : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid VariationId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariationName { get; private set; } = string.Empty;
    public Sku Sku { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public OrderProductCatalogStatus Status { get; private set; } = OrderProductCatalogStatus.Active;

    public bool IsOrderable => Status == OrderProductCatalogStatus.Active;

    private OrderProductCatalog() { }

    public static OrderProductCatalog Create(
        Guid productId,
        Guid variationId,
        string productName,
        string variationName,
        Sku sku,
        Money price,
        OrderProductCatalogStatus status)
    {
        return new OrderProductCatalog
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            VariationId = variationId,
            ProductName = productName,
            VariationName = variationName,
            Sku = sku,
            Price = price,
            Status = status,
        };
    }

    public void UpdateSku(Sku sku)
    {
        Sku = sku;
    }

    public void UpdatePricing(Money price)
    {
        Price = price;
    }

    public void UpdateName(string name)
    {
        ProductName = name;
    }

    public void UpdateStatus(OrderProductCatalogStatus status)
    {
        Status = status;
    }
}
