using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Services;

namespace Inventory.Application.Features.Inventories.Commands.TransferInventory;

public sealed class TransferInventoryHandler(
    TransferService transferService,
    IUnitOfWork unitOfWork,
    IAppLogger<TransferInventoryHandler> logger) : ICommandHandler<TransferInventoryCommand, TransferInventoryResponse>
{
    public async Task<TransferInventoryResponse> Handle(TransferInventoryCommand request, CancellationToken ct = default)
    {
        TransferService.TransferResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var items = request.Items
                .Select(i => new TransferService.TransferItem(i.ProductVariantId, i.Quantity))
                .ToList();

            result = await transferService.TransferAsync(
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                items,
                request.Reason,
                ct);

            var totalQuantity = request.Items.Sum(i => i.Quantity);

            logger.Information(
                "Transferred {ItemCount} items ({TotalQuantity} units) from warehouse {Source} to {Destination}. Doc: {DocNumber}",
                request.Items.Count,
                totalQuantity,
                request.SourceWarehouseId,
                request.DestinationWarehouseId,
                result.SourceDocument.Number);
        }, ct: ct);

        var transferId = result!.SourceDocument.Number;
        var totalQty = request.Items.Sum(i => i.Quantity);

        return new TransferInventoryResponse(
            TransferId: transferId,
            ItemsTransferred: request.Items.Count,
            TotalQuantity: totalQty);
    }
}
