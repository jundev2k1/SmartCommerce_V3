namespace Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed record CycleCountItemRequest(
    Guid ProductVariantId,
    int ActualQuantity);

public sealed record CompleteCycleCountCommand(
    Guid CountId,
    IReadOnlyList<CycleCountItemRequest> CountedItems,
    decimal VarianceThresholdPercent = 5m) : ICommand<CompleteCycleCountResponse>;

public sealed record VarianceItem(
    string ProductVariantId,
    int ExpectedQuantity,
    int ActualQuantity,
    int Variance,
    decimal VariancePercent);

public sealed record CompleteCycleCountResponse(
    int TotalItemsCounted,
    int ItemsWithVariance,
    int ItemsAdjusted,
    decimal TotalVarianceValue,
    IReadOnlyList<VarianceItem> Variances);
