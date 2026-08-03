namespace SmartEcommerce.Inventory.Application.Features.Inventories.DTOs;

public sealed record CreateInventoryTransactionDto(
    Guid InventoryId,
    Guid ProductId,
    Guid ProductVariantId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason);
