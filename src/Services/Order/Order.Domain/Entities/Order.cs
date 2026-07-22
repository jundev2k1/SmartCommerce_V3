using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;
using BuildingBlock.SharedKernel.Text;

using Order.Domain.Enums;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>, IAuditable
{
    public Guid CustomerId { get; private set; }

    /// <summary>Point-in-time snapshot captured at Create time - like OrderItem.ProductName/UnitPrice, never resynced from the User service afterward.</summary>
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string CustomerPhoneSearch { get; private set; } = string.Empty;
    public string CustomerPhoneReverse { get; private set; } = string.Empty;

    /// <summary>Free-text shipping address, same snapshot convention as CustomerName/CustomerPhone - captured once at Create time, never resynced or normalized into structured fields.</summary>
    public string ShippingAddress { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    public ICollection<OrderItem> Items { get; private set; } = [];

    /// <summary>Client-supplied dedup key (scoped per CustomerId - see OrderConfig's unique index) so a retried/double-submitted CreateOrder request doesn't create a second order.</summary>
    public string? IdempotencyKey { get; private set; }

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
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            IdempotencyKey = idempotencyKey,
        };
        order.SyncCustomerSearchFields();

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

    private void SyncCustomerSearchFields()
    {
        CustomerPhoneSearch = PhoneNormalizer.Normalize(CustomerPhone);
        CustomerPhoneReverse = PhoneNormalizer.Reverse(CustomerPhoneSearch);
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
