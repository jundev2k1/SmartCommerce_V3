namespace Inventory.Application.Abstractions.Services;

public interface IInventoryTransactionService
{
    Task RecordAsync(
        Guid inventoryId,
        Guid productId,
        Guid productVariantId,
        Guid warehouseId,
        InventoryTransactionType type,
        int quantity,
        int balanceAfter,
        string reason,
        CancellationToken ct = default);

    Task RecordBatchAsync(
        IReadOnlyList<(
            Guid InventoryId,
            Guid ProductId,
            Guid ProductVariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)> transactions,
        CancellationToken ct = default);
}
