using Inventory.Application.Features.Inventories.DTOs;

namespace Inventory.Application.Abstractions.Persistence.InventoryTransactions;


public interface IInventoryTransactionWriteService
{
    Task StageAddAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default);
}
