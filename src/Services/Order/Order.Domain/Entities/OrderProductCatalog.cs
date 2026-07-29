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
    public const string ActiveStatus = "Active";

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    /// <summary>Mirrors Product's ProductVariationStatus (Active/Inactive/Discontinued) as of the last synced event - the first validation layer CreateOrderHandler checks before trusting a request. See docs/services/order-service.md.</summary>
    public string Status { get; private set; } = ActiveStatus;

    public bool IsOrderable => Status == ActiveStatus;

    private OrderProductCatalog() { }

    public static OrderProductCatalog Create(Guid productVariationId, Guid productId, string productName, string sku, decimal price, string status)
    {
        return new OrderProductCatalog
        {
            Id = productVariationId,
            ProductId = productId,
            ProductName = productName,
            Sku = sku,
            Price = price,
            Status = status,
        };
    }

    public void UpdatePricing(string sku, decimal price, string status)
    {
        Sku = sku;
        Price = price;
        Status = status;
    }

    public void UpdateProductName(string productName)
    {
        ProductName = productName;
    }
}
