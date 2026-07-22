using BuildingBlock.Domain.Abstractions;

using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public sealed class InventoryTransaction : BaseEntity<Guid>, IAuditable
{
    public Guid InventoryId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductVariationId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public InventoryTransactionType Type { get; private set; }
    public int Quantity { get; private set; }
    public int QuantityAfter { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private InventoryTransaction() { }

    public static InventoryTransaction Create(
        Guid id,
        Guid inventoryId,
        Guid productId,
        Guid productVariationId,
        Guid warehouseId,
        InventoryTransactionType type,
        int quantity,
        int quantityAfter,
        string reason)
    {
        return new InventoryTransaction
        {
            Id = id,
            InventoryId = inventoryId,
            ProductId = productId,
            ProductVariationId = productVariationId,
            WarehouseId = warehouseId,
            Type = type,
            Quantity = quantity,
            QuantityAfter = quantityAfter,
            Reason = reason,
        };
    }
}
