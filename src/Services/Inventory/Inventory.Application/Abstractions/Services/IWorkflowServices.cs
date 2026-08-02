namespace Inventory.Application.Abstractions.Services;

/// <summary>Stock deduction workflow: validates, deducts, records, documents.</summary>
public interface IStockDeductionService
{
    Task<StockDeductionResult> DeductAsync(
        string documentNumber,
        InventoryDocumentType documentType,
        InventoryDocumentReason documentReason,
        Guid sourceWarehouseId,
        IReadOnlyList<(Guid InventoryId, Guid ProductId, Guid ProductVariantId, int Quantity)> items,
        string description,
        CancellationToken ct = default);

    public sealed record StockDeductionResult(
        bool Success,
        InventoryDocument Document,
        IReadOnlyList<InventoryStock> DeductedInventories);
}

/// <summary>Inventory adjustment workflow: validates, adjusts, records, documents.</summary>
public interface IInventoryAdjustmentService
{
    Task<AdjustmentResult> AdjustToAsync(
        Guid inventoryId,
        int newQuantity,
        string reason,
        CancellationToken ct = default);

    Task<AdjustmentResult> ReceiveAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default);

    Task<AdjustmentResult> IssueAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default);

    public sealed record AdjustmentResult(
        InventoryStock Inventory,
        int Delta,
        InventoryDocument Document);
}

/// <summary>Receiving workflow: multi-item receiving with lot tracking.</summary>
public interface IReceivingService
{
    Task<ReceivingResult> ReceiveAsync(
        string purchaseOrderNumber,
        Guid warehouseId,
        IReadOnlyList<ReceivingItem> items,
        string description,
        CancellationToken ct = default);

    public sealed record ReceivingItem(
        Guid ProductVariantId,
        Guid WarehouseId,
        int Quantity,
        string? LotNumber = null,
        DateTime? ManufactureDate = null,
        DateTime? ExpiryDate = null);

    public sealed record ReceivingResult(
        InventoryDocument Document,
        IReadOnlyList<InventoryStock> ReceivedInventories,
        IReadOnlyList<InventoryLot> CreatedLots);
}

/// <summary>Warehouse transfer workflow: atomic two-way transfer.</summary>
public interface ITransferService
{
    Task<TransferResult> TransferAsync(
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        IReadOnlyList<TransferItem> items,
        string reason,
        CancellationToken ct = default);

    public sealed record TransferItem(
        Guid ProductVariantId,
        int Quantity);

    public sealed record TransferResult(
        InventoryDocument SourceDocument,
        InventoryDocument DestinationDocument,
        IReadOnlyList<InventoryStock> SourceInventories,
        IReadOnlyList<InventoryStock> DestinationInventories);
}

/// <summary>Cycle count workflow: variance calculation and auto-adjustment.</summary>
public interface ICycleCountService
{
    Task<InventoryCount> StartCountAsync(
        Guid warehouseId,
        string countDate,
        string description,
        CancellationToken ct = default);

    Task<CycleCountResult> CompleteCountAsync(
        Guid countId,
        IReadOnlyList<CountItem> countedItems,
        decimal varianceThresholdPercent = 5m,
        CancellationToken ct = default);

    public sealed record CountItem(
        Guid ProductVariantId,
        int ActualQuantity);

    public sealed record CountVariance(
        Guid InventoryId,
        Guid ProductVariantId,
        int ExpectedQuantity,
        int ActualQuantity,
        int Variance,
        decimal VariancePercent);

    public sealed record CycleCountResult(
        InventoryCount CountDocument,
        IReadOnlyList<CountVariance> Variances,
        IReadOnlyList<InventoryDocument> AdjustmentDocuments,
        int ItemsWithVariance,
        int ItemsAdjusted);
}
