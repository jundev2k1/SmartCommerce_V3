using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class OrderShipping : BaseEntity<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public string ReceiverName { get; private set; } = string.Empty;
    public string ReceiverPhone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public Money ShippingFee { get; private set; } = default!;
    public ShippingMethod ShippingMethod { get; private set; } = ShippingMethod.Standard;
    public ShippingStatus Status { get; private set; } = ShippingStatus.Pending;
    public string Note { get; private set; } = string.Empty;
    public DateTime? ShippedAt { get; private set; }
    public DateTime? ArrivedAtWarehouseAt { get; private set; }
    public DateTime? InTransitAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private OrderShipping() { }

    public static OrderShipping Create(
        Guid orderId,
        string receiverName,
        string receiverPhone,
        string address,
        ShippingMethod shippingMethod,
        Money shippingFee,
        string note)
    {
        return new OrderShipping
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            ReceiverName = receiverName,
            ReceiverPhone = receiverPhone,
            Address = address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ShippingFee = shippingFee,
            ShippingMethod = shippingMethod,
            Note = note,
        };
    }

    public void UpdateContact(
        string receiverName,
        string receiverPhone,
        string address)
    {
        ReceiverName = receiverName;
        ReceiverPhone = receiverPhone;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkShipped()
    {
        if (Status != ShippingStatus.Pending)
            throw new InvalidOperationException($"Cannot mark shipping as shipped when status is {Status}.");

        Status = ShippingStatus.Shipped;

        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkArrivedAtWarehouse()
    {
        if (Status != ShippingStatus.Shipped)
            throw new InvalidOperationException($"Cannot mark shipping as arrived at warehouse when status is {Status}.");

        Status = ShippingStatus.Arrived;

        ArrivedAtWarehouseAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkInTransit()
    {
        if (Status != ShippingStatus.Arrived)
            throw new InvalidOperationException($"Cannot mark shipping as in transit when status is {Status}.");

        Status = ShippingStatus.InTransit;

        InTransitAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        if (Status != ShippingStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark shipping as delivered when status is {Status}.");

        Status = ShippingStatus.Delivered;

        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ShippingStatus.Delivered)
            throw new InvalidOperationException($"Cannot cancel shipping when status is {Status}.");

        Status = ShippingStatus.Canceled;

        UpdatedAt = DateTime.UtcNow;
    }
}
