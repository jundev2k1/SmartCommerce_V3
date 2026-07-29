using Order.Domain.Enums;
using Order.Domain.Metadata;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class OrderDiscount : BaseEntity<Guid>, IAuditable
{
    public Guid OrderId { get; private set; }
    public DiscountTarget Target { get; private set; }
    public DiscountSource Source { get; private set; } = DiscountSource.Unknown;
    public string? SourceId { get; private set; }
    public string SourceCode { get; private set; } = string.Empty;
    public string SourceName { get; private set; } = string.Empty;
    public DiscountMethod Method { get; private set; } = DiscountMethod.FixedAmount;
    public Money Amount { get; private set; } = default!;
    public Money AppliedAmount { get; private set; } = default!;
    public DiscountMetadata? Metadata { get; private set; }

    private OrderDiscount() { }

    public static OrderDiscount Create(
        Guid id,
        Guid orderId,
        DiscountTarget target,
        DiscountSource source,
        string? sourceId,
        string code,
        Money amount,
        DiscountMetadata? metadata = null)
    {
        return new OrderDiscount
        {
            Id = id,
            OrderId = orderId,
            Target = target,
            Source = source,
            SourceId = sourceId,
            SourceCode = code,
            Amount = amount,
            Metadata = metadata,
        };
    }

    public void Apply(Money appliedAmount)
    {
        AppliedAmount = appliedAmount;
    }
}
