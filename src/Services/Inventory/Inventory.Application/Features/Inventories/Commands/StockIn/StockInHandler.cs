using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Features.Inventories.Commands.StockIn;

public sealed class StockInHandler(
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<StockInCommand, StockInResponse>
{
    public async Task<StockInResponse> Handle(StockInCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.StageUpdateAsync(request.InventoryId, async (inv) =>
            {
                inv.Increase(request.Quantity);
                inventory = inv;
                await Task.CompletedTask;
            }, ct);

            await transactionWriteService.StageAddAsync(
                InventoryTransaction.Create(
                    inventory!.Id,
                    inventory.ProductId,
                    inventory.VariationId,
                    inventory.WarehouseId,
                    InventoryTransactionType.StockIn,
                    request.Quantity,
                    inventory.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new StockInResponse(inventory!.Quantity);
    }
}
