namespace Inventory.Application.Features.Inventories.Commands.DeductStock;

/// <summary>
/// DeductionId is the idempotency key for this whole batch (Order Service passes its OrderId).
/// All items deduct together or none do. Tracked as InventoryDocument with Number = DeductionId.ToString().
/// </summary>
public sealed record DeductStockCommand(
    Guid DeductionId,
    IReadOnlyCollection<DeductStockItem> Items,
    string? Reason = null) : ICommand<DeductStockResult>;

public sealed record DeductStockItem(Guid ProductVariationId, int Quantity);

public sealed record InsufficientStockItem(Guid ProductVariationId, int RequestedQuantity, int AvailableQuantity);

public sealed record DeductStockResult(
    bool Success,
    string? FailureCode,
    IReadOnlyCollection<InsufficientStockItem> InsufficientItems);
