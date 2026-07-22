using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockHandler(
    IInventoryRepository inventoryRepo,
    IInventoryTransactionRepository transactionRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;
        var delta = 0;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryRepo.UpdateAsync(request.InventoryId, async (inv) =>
            {
                delta = request.NewQuantity - inv.Quantity;
                inv.Adjust(request.NewQuantity);
                inventory = inv;
                await Task.CompletedTask;
            }, ct);

            await transactionRepo.AddAsync(
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
