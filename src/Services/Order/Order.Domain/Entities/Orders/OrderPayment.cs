namespace NovaCore.Order.Domain.Entities.Orders;

/// <summary>
/// Reference + snapshot of this order's payment - not the payment's system of record.
/// Transactions/capturing/authorization/refund/webhook/retry/gateway-response handling belong to
/// a future Payment Service; this row only ever holds whatever that service last reported, via
/// RecordPayment (same "wholesale snapshot sync" shape as OrderShipping.UpdateSnapshot). 1:1 with
/// Order, sharing its primary key (OrderId) - see OrderPaymentConfig, same pattern as OrderOwner/
/// OrderPrice.
/// </summary>
public sealed class OrderPayment : BaseEntity, IAuditable, ITenantEntity
{
    public Guid OrderId { get; private set; }

    /// <summary>Id of the payment record in the (future) Payment Service - null until that service exists and reports one.</summary>
    public Guid? PaymentReferenceId { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public PaymentProvider? PaymentProvider { get; private set; }
    public string? ProviderName { get; private set; }
    public string? MaskedAccount { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public Money PaidAmount { get; private set; } = default!;
    public string CurrencyCode { get; private set; } = "USD";
    public DateTime? PaidAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderPayment() { }

    /// <summary>Only Order may construct its Payment - same reasoning as OrderOwner.Create being internal.</summary>
    internal static OrderPayment Create(Guid orderId, string currencyCode = "USD")
    {
        return new OrderPayment
        {
            OrderId = orderId,
            PaymentStatus = PaymentStatus.Pending,
            PaidAmount = Money.Create(0),
            CurrencyCode = currencyCode,
        };
    }

    #region Snapshot sync
    /// <summary>
    /// Replaces the whole snapshot at once, mirroring OrderShipping.UpdateSnapshot - Order
    /// Service does not own payment state transitions, it only records the Payment Service's
    /// latest report. Not yet called from anywhere (no Payment Service exists to call it from).
    /// </summary>
    internal void RecordPayment(
        Guid? paymentReferenceId,
        PaymentMethod? paymentMethod,
        PaymentProvider? paymentProvider,
        string? providerName,
        string? maskedAccount,
        string? referenceNumber,
        PaymentStatus paymentStatus,
        Money paidAmount,
        DateTime? paidAt)
    {
        PaymentReferenceId = paymentReferenceId;
        PaymentMethod = paymentMethod;
        PaymentProvider = paymentProvider;
        ProviderName = providerName;
        MaskedAccount = maskedAccount;
        ReferenceNumber = referenceNumber;
        PaymentStatus = paymentStatus;
        PaidAmount = paidAmount;
        PaidAt = paidAt;
    }
    #endregion
}
