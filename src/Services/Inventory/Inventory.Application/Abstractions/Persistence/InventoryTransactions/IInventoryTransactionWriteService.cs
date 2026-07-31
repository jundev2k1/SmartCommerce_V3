using Inventory.Application.Features.Inventories.DTOs;

namespace Inventory.Application.Abstractions.Persistence.InventoryTransactions;

public sealed record CreateInventoryTransactionRequest(
    Guid InventoryId,
    Guid ProductId,
    Guid VariationId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason);

public interface IInventoryTransactionWriteService
{
    Task StageAddAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default);
}
