using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

/// <summary>
/// The element type Order.Create's bulk factory accepts - not a Spec/DTO object reducing
/// parameter count (see conventions/domain-coding-conventions.md#2), since a collection of N
/// structured items has no flat-parameter equivalent. Not persisted itself - Order.Create() turns
/// each model into an owned OrderItem entity.
/// </summary>
public sealed class OrderItem : BaseEntity<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;
    public Money DiscountAmount { get; private set; } = default!;
    public decimal LineTotal => (UnitPrice.Value * Quantity.Value) - DiscountAmount.Value;
    public ICollection<OrderDiscount> Discounts { get; private set; } = [];

    private OrderItem() { }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        Guid variationId,
        string productName,
        Money unitPrice,
        Quantity quantity)
    {
        return new OrderItem
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            ProductId = productId,
            VariationId = variationId,
            Name = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
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
                "Cannot add an Order-targeted discount to the order itself - it must be added to the specific Order instead.");

        Discounts.Add(discount);
    }

    public void AddRangeDiscount(IEnumerable<OrderDiscount> discounts)
    {
        foreach (var discount in discounts)
            AddDiscount(discount);
    }

    public void ApplyDiscount(Money discountAmount)
    {
        DiscountAmount = discountAmount;
    }
}
