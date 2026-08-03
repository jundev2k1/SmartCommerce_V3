using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;

using SmartEcommerce.Inventory.Application.Abstractions.Services;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Commands.ReceiveInventory;

public sealed class ReceiveInventoryHandler(
    IReceivingService receivingService,
    IUnitOfWork unitOfWork,
    IAppLogger<ReceiveInventoryHandler> logger) : ICommandHandler<ReceiveInventoryCommand, ReceiveInventoryResponse>
{
    public async Task<ReceiveInventoryResponse> Handle(ReceiveInventoryCommand request, CancellationToken ct = default)
    {
        IReceivingService.ReceivingResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var items = request.Items
                .Select(i => new IReceivingService.ReceivingItem(
                    ProductVariantId: i.ProductVariantId,
                    WarehouseId: request.WarehouseId,
                    Quantity: i.Quantity,
                    LotNumber: i.LotNumber,
                    ManufactureDate: i.ManufactureDate,
                    ExpiryDate: i.ExpiryDate))
                .ToList();

            result = await receivingService.ReceiveAsync(
                request.PurchaseOrderNumber,
                request.WarehouseId,
                items,
                request.Description,
                ct);

            logger.Information(
                "Received inventory from PO {PoNumber}: {ItemCount} items, {LotCount} lots created",
                request.PurchaseOrderNumber,
                request.Items.Count,
                result.CreatedLots.Count);
        }, ct: ct);

        return new ReceiveInventoryResponse(
            ItemsReceived: result!.ReceivedInventories.Count,
            LotsCreated: result.CreatedLots.Count);
    }
}
