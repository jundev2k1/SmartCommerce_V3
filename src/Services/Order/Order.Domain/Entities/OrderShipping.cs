namespace Order.Domain.Entities;

public sealed class OrderShipping : BaseEntity<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = default!;
    public string Address { get; private set; } = string.Empty;
    public Money OriginalFee { get; private set; } = default!;
    public Money DiscountAmount { get; private set; } = default!;
    public Money FinalFee { get; private set; } = default!;
    public ShippingMethod ShippingMethod { get; private set; } = ShippingMethod.Standard;
    public ShippingStatus Status { get; private set; } = ShippingStatus.Pending;
    public string Note { get; private set; } = string.Empty;
    public DateTime? ShippedAt { get; private set; }
    public DateTime? ArrivedAtWarehouseAt { get; private set; }
    public DateTime? InTransitAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? IdempotencyKey { get; private set; }

    #region Constructor
    private OrderShipping() { }

    public static OrderShipping Create(
        Guid orderId,
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        ShippingMethod shippingMethod,
        string note,
        string? idempotencyKey = null)
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
            ShippingMethod = shippingMethod,
            Note = note,
            IdempotencyKey = idempotencyKey,
        };
    }
    #endregion

    #region Shipping information
    public void UpdateContact(
        string receiverName,
        PhoneNumber receiverPhone,
        string address,
        string idempotencyKey)
    {
        if (Status is not ShippingStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Receiver ({Status}) cannot be modified while the Order is being delivering.");

        ReceiverName = receiverName;
        ReceiverPhone = receiverPhone;
        Address = address;
        IdempotencyKey = idempotencyKey;
    }

    public void UpdateNote(string note)
    {
        if (Status is ShippingStatus.Delivered or ShippingStatus.Canceled)
            throw ExceptionFactory.InvalidStatus($"Note cannot be edited once the Order is completed.");

        Note = note;
    }
    #endregion

    #region Shipping progress
    public void MarkShipped()
    {
        if (Status != ShippingStatus.Pending)
            throw new InvalidOperationException($"Cannot mark shipping as shipped when status is {Status}.");

        Status = ShippingStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkArrivedAtWarehouse()
    {
        if (Status != ShippingStatus.Shipped)
            throw new InvalidOperationException($"Cannot mark shipping as arrived at warehouse when status is {Status}.");

        Status = ShippingStatus.Arrived;
        ArrivedAtWarehouseAt = DateTime.UtcNow;
    }

    public void MarkInTransit()
    {
        if (Status != ShippingStatus.Arrived)
            throw new InvalidOperationException($"Cannot mark shipping as in transit when status is {Status}.");

        Status = ShippingStatus.InTransit;
        InTransitAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        if (Status != ShippingStatus.InTransit)
            throw new InvalidOperationException($"Cannot mark shipping as delivered when status is {Status}.");

        Status = ShippingStatus.Delivered;

        DeliveredAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ShippingStatus.Delivered)
            throw new InvalidOperationException($"Cannot cancel shipping when status is {Status}.");

        Status = ShippingStatus.Canceled;
    }
    #endregion

    #region Pricing
    internal void Calculate()
    {
        OriginalFee = Money.Create(0);
        DiscountAmount = Money.Create(0);

        var shippingFee = OriginalFee.Value + DiscountAmount.Value;
        FinalFee = Money.Create(shippingFee);
    }
    #endregion
}
