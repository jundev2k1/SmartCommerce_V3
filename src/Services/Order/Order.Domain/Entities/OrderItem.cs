using BuildingBlock.Application.Exceptions;

namespace Order.Domain.Entities;

public sealed class OrderItem : BaseEntity<long>, IAuditable
{
    public Guid OrderId { get; private set; }
    public int OrderItemNo { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariationId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string VariationName { get; private set; } = string.Empty;
    public OrderItemType Type { get; private set; }
    public Money UnitPrice { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;
    /// <summary>Total amount before any discount (UnitPrice × Quantity)</summary>
    public Money Subtotal { get; private set; } = default!;
    /// <summary>Total discount from product promotion.</summary>
    public Money ProductPromotionDiscount { get; private set; } = default!;
    /// <summary>Total discount allocated from bundle/set promotion.</summary>
    public Money BundleDiscount { get; private set; } = default!;
    /// <summary>Total discount allocated from gift campaigns.</summary>
    public Money GiftDiscount { get; private set; } = default!;
    /// <summary>Total discount from item-level coupon.</summary>
    public Money CouponDiscount { get; private set; } = default!;
    /// <summary>Manual adjustment made by administrator.</summary>
    public Money ManualDiscount { get; private set; } = default!;
    /// <summary>Final amount contributed by this item.</summary>
    public Money FinalAmount { get; private set; } = default!;

    public ICollection<OrderDiscount> Discounts { get; private set; } = [];

    private OrderItem() { }

    internal static OrderItem Create(
        Guid orderId,
        int orderItemNo,
        Guid productId,
        Guid variationId,
        string productName,
        string variationName,
        Money unitPrice,
        Quantity quantity,
        OrderItemType type = OrderItemType.Normal)
    {
        return new OrderItem
        {
            OrderId = orderId,
            OrderItemNo = orderItemNo,
            ProductId = productId,
            VariationId = variationId,
            ProductName = productName,
            VariationName = variationName,
            Type = type,
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }

    internal void LoadDiscounts(IEnumerable<OrderDiscount> discounts)
    {
        if (discounts.Any(d => !d.OrderItemId.HasValue || d.OrderItemId != Id))
            throw new BadRequestException("One or more order item discount are invalid.");

        Discounts = [.. discounts];
    }

    public void AddDiscount(OrderDiscount discount)
    {
        if (discount.OrderId != OrderId)
            throw new InvalidArgumentException(
                "Cannot add a discount to an orderItem that does not match the discount's OrderId.");

        if (!discount.OrderItemId.HasValue || discount.OrderItemId.Value != Id)
            throw new InvalidArgumentException(
                "Cannot add a discount to an orderItem that does not match the discount's OrderItemId");

        if (discount.Target != DiscountTarget.OrderItem)
            throw new InvalidArgumentException(
                "Cannot add an Order-targeted discount to the order item itself - it must be added to the specific Order instead.");

        Discounts.Add(discount);
    }

    public void AddRangeDiscount(IEnumerable<OrderDiscount> discounts)
    {
        foreach (var discount in discounts)
            AddDiscount(discount);
    }

    public void ApplyProductPromotionDiscount(Money discountAmount)
    {
        ProductPromotionDiscount = discountAmount;
    }
}
