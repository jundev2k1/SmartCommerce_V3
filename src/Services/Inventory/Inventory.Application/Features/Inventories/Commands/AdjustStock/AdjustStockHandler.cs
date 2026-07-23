using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

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
                    Guid.CreateVersion7(),
                    inventory!.Id,
                    inventory.ProductId,
                    inventory.ProductVariationId,
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
