using SmartEcommerce.Inventory.Application.Features.Inventories.DTOs;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryTransactions;


public interface IInventoryTransactionWriteService
{
    Task StageAddAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default);
}
