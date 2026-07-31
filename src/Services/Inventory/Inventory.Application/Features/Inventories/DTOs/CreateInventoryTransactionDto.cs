namespace Inventory.Application.Features.Inventories.DTOs;

public sealed record CreateInventoryTransactionDto(
    Guid InventoryId,
    Guid ProductId,
    Guid VariationId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityBefore,
    int QuantityAfter,
    string Reason);
