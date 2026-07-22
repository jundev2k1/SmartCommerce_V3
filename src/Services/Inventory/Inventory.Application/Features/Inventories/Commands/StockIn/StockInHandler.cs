using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Commands.StockIn;

public sealed class StockInHandler(
    IInventoryRepository inventoryRepo,
    IInventoryTransactionRepository transactionRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<StockInCommand, StockInResponse>
{
    public async Task<StockInResponse> Handle(StockInCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryRepo.UpdateAsync(request.InventoryId, async (inv) =>
            {
                inv.Increase(request.Quantity);
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
                    InventoryTransactionType.StockIn,
                    request.Quantity,
                    inventory.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new StockInResponse(inventory!.Quantity);
    }
}
