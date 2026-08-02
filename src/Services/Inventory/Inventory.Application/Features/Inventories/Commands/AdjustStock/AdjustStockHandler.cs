using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Services;

namespace Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockHandler(
    InventoryAdjustmentService adjustmentService,
    IUnitOfWork unitOfWork,
    IAppLogger<AdjustStockHandler> logger) : ICommandHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct = default)
    {
        InventoryAdjustmentService.AdjustmentResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            result = await adjustmentService.AdjustToAsync(
                request.InventoryId,
                request.NewQuantity,
                request.Reason.Trim(),
                ct);

            logger.Information(
                "Adjusted inventory {InventoryId} to {NewQuantity} (delta: {Delta})",
                request.InventoryId, request.NewQuantity, result.Delta);
        }, ct: ct);

        return new AdjustStockResponse(result!.Inventory.AvailableQuantity);
    }
}
