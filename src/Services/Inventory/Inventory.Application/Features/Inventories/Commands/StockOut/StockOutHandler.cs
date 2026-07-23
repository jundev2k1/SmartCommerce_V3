using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed class StockOutHandler(
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IUnitOfWork unitOfWork) : ICommandHandler<StockOutCommand, StockOutResponse>
{
    public async Task<StockOutResponse> Handle(StockOutCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.StageUpdateAsync(request.InventoryId, async (inv) =>
            {
                inv.Decrease(request.Quantity);
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
                    InventoryTransactionType.StockOut,
                    request.Quantity,
                    inventory.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new StockOutResponse(inventory!.Quantity);
    }
}
