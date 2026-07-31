using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;

namespace Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockHandler(
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;
        var delta = 0;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.StageUpdateAsync(request.InventoryId, async (inv) =>
            {
                delta = request.NewQuantity - inv.Quantity;
                inv.Adjust(request.NewQuantity);
                inventory = inv;
                await Task.CompletedTask;
            }, ct);

            await transactionWriteService.StageAddAsync(
                InventoryTransaction.Create(
                    inventory!.Id,
                    inventory.ProductId,
                    inventory.VariationId,
                    inventory.WarehouseId,
                    InventoryTransactionType.Adjustment,
                    delta,
                    inventory.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new AdjustStockResponse(inventory!.Quantity);
    }
}
