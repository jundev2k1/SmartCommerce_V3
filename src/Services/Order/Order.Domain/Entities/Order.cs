using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;

using Order.Domain.Enums;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>, IAuditable
{
    /// <summary>Who placed this order and where it ships - see OrderOwner remarks.</summary>
    public OrderOwner Owner { get; private set; } = default!;

    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    public ICollection<OrderItem> Items { get; private set; } = [];

    /// <summary>Set only when Status is Cancelled - e.g. "OutOfStock" (CreateOrderSaga compensation) or "CancelledByCustomer" (manual CancelOrder).</summary>
    public string? CancellationReason { get; private set; }

    private Order() { }

    public static Order Create(
        Guid id, Guid customerId, string customerName, string customerPhone, string shippingAddress,
        IEnumerable<OrderItemCreateModel> items, string? idempotencyKey = null)
    {
        var models = items.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("An order must contain at least one item.");

        var order = new Order
        {
            Id = id,
            Status = OrderStatus.Pending,
        };
        order.Owner = OrderOwner.Create(id, customerId, customerName, customerPhone, shippingAddress, idempotencyKey);

        foreach (var model in models)
        {
            order.Items.Add(OrderItem.Create(
                Guid.CreateVersion7(), id, model.ProductId, model.ProductName, model.UnitPrice, model.Quantity, model.Discount));
        }

        return order;
    }

    /// <summary>Replaces the item list wholesale - only while the order hasn't left the customer-editable Pending window (Confirm() moves it out via the async CreateOrderSaga).</summary>
    public void UpdateItems(IEnumerable<OrderItemCreateModel> items)
    {
        if (Status != OrderStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot update items on an order in {Status} status.");

        var models = items.ToList();
        if (models.Count == 0)
            throw ExceptionFactory.EmptyCollection("An order must contain at least one item.");

        Items.Clear();
        foreach (var model in models)
        {
            Items.Add(OrderItem.Create(
                Guid.CreateVersion7(), Id, model.ProductId, model.ProductName, model.UnitPrice, model.Quantity, model.Discount));
        }

        Tourch();
    }

    /// <summary>Updates the customer-editable contact/shipping snapshot - allowed in the same non-terminal window as Cancel(), since a Cancelled/Completed order no longer ships.</summary>
    public void UpdateOwnerInfo(string customerPhone, string shippingAddress)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
            throw ExceptionFactory.InvalidStatus($"Cannot update owner information on an order in {Status} status.");

        Owner.UpdateContact(customerPhone, shippingAddress);
        Tourch();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot confirm an order in {Status} status.");

        Status = OrderStatus.Confirmed;
        Tourch();
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("A cancellation reason is required.");

        if (Status == OrderStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Order is already cancelled.");

        if (Status == OrderStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed order.");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason.Trim();
        Tourch();
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw ExceptionFactory.InvalidStatus($"Cannot complete an order in {Status} status.");

        Status = OrderStatus.Completed;
        Tourch();
    }
}
