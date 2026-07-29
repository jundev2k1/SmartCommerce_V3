using BuildingBlock.SharedKernel.Extensions;
using BuildingBlock.SharedKernel.Text;

using Order.Domain.ValueObjects;

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
    public Guid OwnerId { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public Email OwnerEmail { get; private set; } = default!;
    public string OwnerPhone { get; private set; } = string.Empty;
    public string OwnerPhoneSearch { get; private set; } = string.Empty;
    public string OwnerPhoneReverse { get; private set; } = string.Empty;

    /// <summary>Client-supplied dedup key (scoped per CustomerId - see OrderOwnerConfig's unique index) so a retried/double-submitted CreateOrder request doesn't create a second order.</summary>
    public string? IdempotencyKey { get; private set; }

    private OrderOwner() { }

    /// <summary>Only Order may construct/mutate its Owner - same reasoning as OrderItem.Create being internal.</summary>
    public static OrderOwner Create(
        Guid orderId,
        Guid customerId,
        string name,
        string phone,
        string? idempotencyKey)
    {
        var owner = new OrderOwner
        {
            OrderId = orderId,
            OwnerId = customerId,
            OwnerName = name,
            OwnerPhone = phone,
            IdempotencyKey = idempotencyKey,
        };
        owner.SyncCustomerSearchFields();

        return owner;
    }

    public void UpdateContact(string ownerName, Email ownerEmail, string ownerPhone)
    {
        if (ownerName.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Owner name cannot be empty.");

        if (ownerPhone.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Owner phone cannot be empty.");

        OwnerName = ownerName;
        OwnerEmail = ownerEmail;
        OwnerPhone = ownerPhone;
        SyncCustomerSearchFields();
    }

    private void SyncCustomerSearchFields()
    {
        OwnerPhoneSearch = PhoneNormalizer.Normalize(OwnerPhone);
        OwnerPhoneReverse = PhoneNormalizer.Reverse(OwnerPhoneSearch);
    }
}
