using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed class StockOutHandler(
    IInventoryRepository inventoryRepo,
    IInventoryTransactionRepository transactionRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<StockOutCommand, StockOutResponse>
{
    public async Task<StockOutResponse> Handle(StockOutCommand request, CancellationToken ct = default)
    {
        InventoryEntity? inventory = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryRepo.UpdateAsync(request.InventoryId, async (inv) =>
            {
                inv.Decrease(request.Quantity);
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
                    InventoryTransactionType.StockOut,
                    request.Quantity,
                    inventory.Quantity,
                    request.Reason.Trim()),
                ct);
        }, ct: ct);

        return new StockOutResponse(inventory!.Quantity);
    }
}
