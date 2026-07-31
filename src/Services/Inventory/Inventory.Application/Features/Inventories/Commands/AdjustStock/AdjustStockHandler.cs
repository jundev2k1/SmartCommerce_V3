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
        InventoryAdjustmentResult? adjustment = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            adjustment = await inventoryWriteService.AdjustToAsync(request.InventoryId, request.NewQuantity, ct);

            await transactionWriteService.StageAddAsync(
                InventoryTransaction.Create(
                    adjustment.Entity.Id,
                    adjustment.Entity.ProductId,
                    adjustment.Entity.VariationId,
                    adjustment.Entity.WarehouseId,
                    InventoryTransactionType.Adjustment,
                    adjustment.Delta,
                    adjustment.Entity.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new AdjustStockResponse(adjustment!.Entity.Quantity);
    }
}
