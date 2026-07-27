using BuildingBlock.Domain.Abstractions;
using BuildingBlock.SharedKernel.Text;

namespace Order.Domain.Entities;

/// <summary>
/// Point-in-time snapshot of who placed an order and where it ships - captured once at Create
/// time, never resynced from the User service afterward (same convention OrderItem.ProductName/
/// UnitPrice already follow). Split out from Order itself so the core order/status/items data
/// isn't coupled to this snapshot's columns. 1:1 with Order, sharing its primary key (OrderId) -
/// see OrderOwnerConfig.
/// </summary>
public sealed class OrderOwner : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string CustomerPhoneSearch { get; private set; } = string.Empty;
    public string CustomerPhoneReverse { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;

    /// <summary>Client-supplied dedup key (scoped per CustomerId - see OrderOwnerConfig's unique index) so a retried/double-submitted CreateOrder request doesn't create a second order.</summary>
    public string? IdempotencyKey { get; private set; }

    private OrderOwner() { }

    /// <summary>Only Order may construct/mutate its Owner - same reasoning as OrderItem.Create being internal.</summary>
    internal static OrderOwner Create(
        Guid orderId, Guid customerId, string customerName, string customerPhone, string shippingAddress, string? idempotencyKey)
    {
        var owner = new OrderOwner
        {
            OrderId = orderId,
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            ShippingAddress = shippingAddress,
            IdempotencyKey = idempotencyKey,
        };
        owner.SyncCustomerSearchFields();

        return owner;
    }

    internal void UpdateContact(string customerPhone, string shippingAddress)
    {
        CustomerPhone = customerPhone;
        ShippingAddress = shippingAddress;
        SyncCustomerSearchFields();
        Tourch();
    }

    private void SyncCustomerSearchFields()
    {
        CustomerPhoneSearch = PhoneNormalizer.Normalize(CustomerPhone);
        CustomerPhoneReverse = PhoneNormalizer.Reverse(CustomerPhoneSearch);
    }
}
