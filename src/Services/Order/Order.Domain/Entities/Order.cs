using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>, IAuditable
{
    public OrderNumber OrderNumber { get; private set; } = default!;
    public OrderOwner Owner { get; private set; } = default!;
    public OrderShipping Shipping { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    public ICollection<OrderItem> Items { get; private set; } = [];
    public ICollection<OrderDiscount> Discounts { get; private set; } = [];
    public string? CancellationReason { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid? CreatedById { get; private set; }

    private Order() { }

    public static Order Create(string idempotencyKey, Guid? createdById = null)
    {
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            OrderNumber = OrderNumber.Create(),
            Status = OrderStatus.Pending,
            CreatedById = createdById,
            IdempotencyKey = idempotencyKey,
        };

        return order;
    }

    public void SetOrderItems(OrderItem[] items)
    {
        if (items.Length == 0)
            throw ExceptionFactory.EmptyCollection("An order must contain at least one item.");

        if (Items.Count != 0)
            throw ExceptionFactory.InvalidState("The items in the order cannot be modified.");

        Items = items;
    }

    public void SetOwner(OrderOwner owner)
    {
        if (owner.OrderId != Id)
            throw new InvalidArgumentException(
                "Cannot set an OrderOwner for an order that does not match the OrderOwner's OrderId.");

        Owner = owner;
    }

    public void SetShipping(OrderShipping shipping)
    {
        if (shipping.OrderId != Id)
            throw new InvalidArgumentException(
                "Cannot set an OrderShipping for an order that does not match the OrderShipping's OrderId.");

        Shipping = shipping;
    }

    public void AddDiscount(OrderDiscount discount)
    {
        if (discount.OrderId != Id)
            throw new InvalidArgumentException(
                "Cannot add a discount to an order that does not match the discount's OrderId.");

        if (discount.Target != DiscountTarget.Order)
            throw new InvalidArgumentException(
                "Cannot add an OrderItem-targeted discount to the order itself - it must be added to the specific OrderItem instead.");

        Discounts.Add(discount);
    }

    public void AddRangeDiscounts(IEnumerable<OrderDiscount> discounts)
    {
        foreach (var discount in discounts)
        {
            AddDiscount(discount);
        }
    }

    public void UpdateOwnerInfo(
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus(
                $"Cannot update owner information on an order in {Status} status.");

        Owner.UpdateContact(ownerName, ownerEmail, ownerPhone, idempotencyKey);
    }

    public void UpdateShippingInfo(
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        string idempotencyKey)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus($"Cannot update shipping information on an order in {Status} status.");

        Shipping.UpdateContact(receiverName, receiverPhone, address, idempotencyKey);
    }

    public void MarkShipped()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot mark an order as shipped when status is {Status}.");

        Shipping.MarkShipped();
    }

    public void MarkArrivedAtWarehouse()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot mark an order as arrived at warehouse when status is {Status}.");

        Shipping.MarkArrivedAtWarehouse();
    }

    public void MarkInTransit()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot mark an order as in transit when status is {Status}.");

        Shipping.MarkInTransit();
    }

    public void MarkDelivered()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot mark an order as delivered when status is {Status}.");

        Shipping.MarkDelivered();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot confirm an order in {Status} status.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A cancellation reason is required.");

        if (Status == OrderStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Order is already cancelled.");

        if (Status == OrderStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed order.");

        Shipping.Cancel();
        Status = OrderStatus.Cancelled;
        CancellationReason = reason.Trim();
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot complete an order in {Status} status.");

        if (Shipping.Status != ShippingStatus.Delivered)
            throw ExceptionFactory.InvalidStatus($"Cannot complete an order when shipping status is {Shipping.Status}.");

        Status = OrderStatus.Completed;
    }
}
