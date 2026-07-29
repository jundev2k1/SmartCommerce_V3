using Order.Domain.Enums;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>, IAuditable
{
    public OrderOwner Owner { get; private set; } = default!;
    public OrderShipping Shipping { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    public ICollection<OrderItem> Items { get; private set; } = [];
    public ICollection<OrderDiscount> Discounts { get; private set; } = [];

    public string? CancellationReason { get; private set; }

    private Order() { }

    public static Order Create(
        Guid id,
        OrderOwner owner,
        OrderShipping shipping,
        IEnumerable<OrderItem> items)
    {
        var models = items.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("An order must contain at least one item.");

        var order = new Order
        {
            Id = id,
            Status = OrderStatus.Pending,
            Owner = owner,
            Shipping = shipping,
            Items = [.. items],
            Discounts = [],
        };

        return order;
    }

    public void AddDiscount(OrderDiscount discount)
    {
        if (discount.OrderId != Id)
            throw new InvalidArgumentException(
                "Cannot add a discount to an order that does not match the discount's OrderId.");

        Discounts.Add(discount);
    }

    /// <summary>Updates the customer-editable contact/shipping snapshot - allowed in the same non-terminal window as Cancel(), since a Cancelled/Completed order no longer ships.</summary>
    public void UpdateOwnerInfo(string customerPhone, string shippingAddress)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus(
                $"Cannot update owner information on an order in {Status} status.");

        Owner.UpdateContact(customerPhone, shippingAddress);
    }

    public void UpdateShippingInfo(string receiverName, string receiverPhone, string address)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus($"Cannot update shipping information on an order in {Status} status.");

        Shipping.UpdateContact(receiverName, receiverPhone, address);
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
