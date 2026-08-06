namespace NovaCore.Promotion.Domain.Entities.Audits;

/// <summary>
/// A platform-level audit row for any Promotion-service aggregate (AggregateId is generic, not
/// scoped to one specific aggregate type) - same shape as every other *History entity in this
/// service (CouponHistory, VoucherHistory, PointHistory, ...), just named "Audit" and not tied to
/// a single owning aggregate. CreatedAt is inherited from BaseEntity, not redeclared. No
/// Properties/Navigation/TenantId was given for this or any other Audit entity, so all four stay
/// plain BaseEntity (not AggregateRoot, no ITenantEntity) - see
/// docs/promotion-service/aggregates/audit.md.
/// </summary>
public sealed class PromotionAudit : BaseEntity<Guid>, IAuditable
{
    public Guid AggregateId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    private PromotionAudit() { }

    public static PromotionAudit Create(Guid aggregateId, string action, Guid? operatorId = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("Audit action cannot be empty.");

        return new PromotionAudit
        {
            Id = Guid.CreateVersion7(),
            AggregateId = aggregateId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
