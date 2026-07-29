using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;

using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

/// <summary>
/// Idempotency ledger for the gRPC DeductStock/RestockStock pair - Id is the caller-supplied
/// deduction_id (Order Service passes its OrderId), not a surrogate key, so a redelivered/retried
/// gRPC call with the same id can look up and replay the original outcome instead of decrementing
/// stock twice. ItemsJson snapshots exactly what was deducted (variation/warehouse/quantity) so
/// RestockStock can reverse it without the caller re-sending item details. gRPC has no built-in
/// Outbox/Inbox the way Kafka consumers do here, so this table is what makes DeductStock/
/// RestockStock safe under saga retries and at-least-once delivery.
/// </summary>
public sealed class StockDeduction : BaseEntity<Guid>
{
    public StockDeductionStatus Status { get; private set; }
    public string ItemsJson { get; private set; } = string.Empty;
    public string? FailureCode { get; private set; }
    public string? Reason { get; private set; }

    private StockDeduction() { }

    public static StockDeduction CreateSucceeded(Guid deductionId, string itemsJson, string? reason)
    {
        if (string.IsNullOrWhiteSpace(itemsJson))
            throw ExceptionFactory.RequiredField("Deducted items snapshot cannot be empty.");

        return new StockDeduction
        {
            Id = deductionId,
            Status = StockDeductionStatus.Succeeded,
            ItemsJson = itemsJson,
            Reason = reason,
        };
    }

    public static StockDeduction CreateFailed(Guid deductionId, string failureCode, string? reason)
    {
        return new StockDeduction
        {
            Id = deductionId,
            Status = StockDeductionStatus.Failed,
            ItemsJson = "[]",
            FailureCode = failureCode,
            Reason = reason,
        };
    }

    public void MarkReversed()
    {
        if (Status != StockDeductionStatus.Succeeded)
            throw ExceptionFactory.InvalidStatus($"Cannot reverse a stock deduction in {Status} status.");

        Status = StockDeductionStatus.Reversed;
    }
}
