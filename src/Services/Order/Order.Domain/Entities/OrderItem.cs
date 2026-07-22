using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;

namespace Order.Domain.Entities;

/// <summary>
/// The element type Order.Create's bulk factory accepts - not a Spec/DTO object reducing
/// parameter count (see conventions/domain-coding-conventions.md#2), since a collection of N
/// structured items has no flat-parameter equivalent. Not persisted itself - Order.Create() turns
/// each model into an owned OrderItem entity.
/// </summary>
public sealed record OrderItemCreateModel(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Discount = 0m);

public sealed class OrderItem : BaseEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Discount { get; private set; }
    public decimal LineTotal => (UnitPrice * Quantity) - Discount;

    private OrderItem() { }

    internal static OrderItem Create(Guid id, Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity, decimal discount = 0m)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Order item quantity must be greater than zero.");

        if (unitPrice < 0)
            throw ExceptionFactory.InvalidRange("Order item unit price cannot be negative.");

        if (discount < 0 || discount > unitPrice * quantity)
            throw ExceptionFactory.InvalidRange("Order item discount must be between 0 and the line's pre-discount total.");

        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Discount = discount,
        };
    }
}
