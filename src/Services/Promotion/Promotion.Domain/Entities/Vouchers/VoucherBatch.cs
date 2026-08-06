namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>
/// A generation/import batch that one or more Vouchers can reference via BatchId - not owned by a
/// single Voucher, so construction is public, not internal. Fields mirror CouponBatch's shape (the
/// prompt that requested this entity did not include its own Fields section - inferred from
/// CouponBatch's parallel role, flagged in docs/promotion-service/aggregates/voucher.md).
/// </summary>
public sealed class VoucherBatch : BaseEntity<Guid>, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    public DateTime? ImportedAt { get; private set; }
    public int TotalCount { get; private set; }
    public int ActivatedCount { get; private set; }
    public int UsedCount { get; private set; }
    public int FailedCount { get; private set; }

    private VoucherBatch() { }

    public static VoucherBatch Create(string name, string? source = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Batch name cannot be empty.");

        return new VoucherBatch
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Source = source,
        };
    }

    /// <summary>Structural counter update only - no import/activation logic lives here.</summary>
    public void RecordImport(DateTime importedAt, int totalCount)
    {
        ImportedAt = importedAt;
        TotalCount = totalCount;
    }

    public void UpdateCounts(int activatedCount, int usedCount, int failedCount)
    {
        ActivatedCount = activatedCount;
        UsedCount = usedCount;
        FailedCount = failedCount;
    }
}
