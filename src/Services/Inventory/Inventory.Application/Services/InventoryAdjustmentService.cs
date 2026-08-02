using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Services;

/// <summary>
/// Owns inventory adjustment workflow: validates inventory, applies adjustment, records transaction, documents change.
/// Reusable across AdjustStock, StockIn, StockOut, and future adjustment operations.
/// </summary>
public sealed class InventoryAdjustmentService(
    IInventoryReadService readService,
    IInventoryWriteService writeService,
    InventoryDocumentService documentService,
    InventoryTransactionService transactionService)
{
    public sealed record AdjustmentResult(
        InventoryStock Inventory,
        int Delta,
        InventoryDocument Document);

    /// <summary>
    /// Adjusts stock to a target quantity and records complete audit trail.
    /// Returns inventory state, delta, and document for caller use.
    /// </summary>
    public async Task<AdjustmentResult> AdjustToAsync(
        Guid inventoryId,
        int newQuantity,
        string reason,
        CancellationToken ct = default)
    {
        var inventory = await readService.GetByIdAsync(inventoryId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Inventory {inventoryId} not found.");

        var oldQuantity = inventory.AvailableQuantity;
        var adjusted = await writeService.AdjustStockAsync(inventoryId, newQuantity, ct);
        var delta = adjusted.Delta;

        var description = $"Quantity adjusted from {oldQuantity} to {newQuantity}. {reason}";
        var document = await documentService.CreateAndCompleteAsync(
            type: InventoryDocumentType.Adjustment,
            reason: InventoryDocumentReason.Adjustment,
            sourceWarehouseId: inventory.WarehouseId,
            destinationWarehouseId: null,
            description: description,
            ct: ct);

        document.AddItem(
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            quantity: Math.Abs(delta),
            unitOfMeasure: "EA",
            inventoryId: inventory.Id,
            description: $"Delta: {delta:+#;-#;0}");

        await transactionService.RecordAsync(
            inventoryId: inventory.Id,
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            warehouseId: inventory.WarehouseId,
            type: InventoryTransactionType.Adjustment,
            quantity: delta,
            balanceAfter: adjusted.Entity.AvailableQuantity,
            reason: reason,
            ct: ct);

        return new AdjustmentResult(adjusted.Entity, delta, document);
    }

    /// <summary>
    /// Receives stock (stock-in operation) and records receipt document.
    /// </summary>
    public async Task<AdjustmentResult> ReceiveAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default)
    {
        var inventory = await readService.GetByIdAsync(inventoryId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Inventory {inventoryId} not found.");

        var updated = await writeService.ReceiveStockAsync(inventoryId, quantity, ct);

        var document = await documentService.CreateAndCompleteAsync(
            type: InventoryDocumentType.Receipt,
            reason: InventoryDocumentReason.Purchase,
            sourceWarehouseId: null,
            destinationWarehouseId: inventory.WarehouseId,
            description: reason,
            ct: ct);

        document.AddItem(
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            quantity: quantity,
            unitOfMeasure: "EA",
            inventoryId: inventory.Id,
            description: reason);

        await transactionService.RecordAsync(
            inventoryId: inventory.Id,
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            warehouseId: inventory.WarehouseId,
            type: InventoryTransactionType.StockIn,
            quantity: quantity,
            balanceAfter: updated.AvailableQuantity,
            reason: reason,
            ct: ct);

        return new AdjustmentResult(updated, quantity, document);
    }

    /// <summary>
    /// Issues stock (stock-out operation) and records issue document.
    /// </summary>
    public async Task<AdjustmentResult> IssueAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default)
    {
        var inventory = await readService.GetByIdAsync(inventoryId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Inventory {inventoryId} not found.");

        var updated = await writeService.DeductStockAsync(inventoryId, quantity, ct);

        var document = await documentService.CreateAndCompleteAsync(
            type: InventoryDocumentType.Issue,
            reason: InventoryDocumentReason.Sale,
            sourceWarehouseId: inventory.WarehouseId,
            destinationWarehouseId: null,
            description: reason,
            ct: ct);

        document.AddItem(
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            quantity: quantity,
            unitOfMeasure: "EA",
            inventoryId: inventory.Id,
            description: reason);

        await transactionService.RecordAsync(
            inventoryId: inventory.Id,
            productId: inventory.ProductId,
            productVariantId: inventory.VariantId,
            warehouseId: inventory.WarehouseId,
            type: InventoryTransactionType.StockOut,
            quantity: quantity,
            balanceAfter: updated.AvailableQuantity,
            reason: reason,
            ct: ct);

        return new AdjustmentResult(updated, -quantity, document);
    }
}
