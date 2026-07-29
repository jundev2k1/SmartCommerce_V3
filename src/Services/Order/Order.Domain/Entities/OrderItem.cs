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
    public string ProductName { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;
    public Money DiscountAmount { get; private set; } = default!;
    public decimal LineTotal => (UnitPrice.Value * Quantity.Value) - DiscountAmount.Value;
    public ICollection<OrderDiscount> Discounts { get; private set; } = [];

    private OrderItem() { }

    public static OrderItem Create(
        Guid id,
        Guid orderId,
        Guid productId,
        string productName,
        Money unitPrice,
        Quantity quantity)
    {
        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }

    public void ApplyDiscount(Money discountAmount)
    {
        DiscountAmount = discountAmount;
    }
}
