using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;

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
            inventory = await inventoryWriteService.DecreaseAsync(request.InventoryId, request.Quantity, ct);

            await transactionWriteService.StageAddAsync(
                InventoryTransaction.Create(
                    inventory.Id,
                    inventory.ProductId,
                    inventory.VariationId,
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
