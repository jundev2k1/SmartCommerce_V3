using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockHandler(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryDocumentWriteService documentWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IUnitOfWork unitOfWork,
    IAppLogger<AdjustStockHandler> logger) : ICommandHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct = default)
    {
        var inventory = await inventoryReadService.GetByIdAsync(request.InventoryId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Inventory {request.InventoryId} not found.");

        InventoryAdjustmentResult? adjustment = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var oldQuantity = inventory.AvailableQuantity;
            adjustment = await inventoryWriteService.AdjustStockAsync(request.InventoryId, request.NewQuantity, ct);
            var delta = adjustment.Delta;

            var description = $"Quantity adjusted from {oldQuantity} to {request.NewQuantity}. {request.Reason}";

            var document = InventoryDocument.Create(
                number: Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant(),
                type: InventoryDocumentType.Adjustment,
                reason: InventoryDocumentReason.Adjustment,
                sourceWarehouseId: inventory.WarehouseId,
                destinationWarehouseId: null,
                description: description);

            document.AddItem(
                productId: inventory.ProductId,
                productVariantId: inventory.VariantId,
                quantity: Math.Abs(delta),
                unitOfMeasure: "EA",
                inventoryId: inventory.Id,
                description: $"Delta: {delta:+#;-#;0}");

            document.Submit();
            document.Approve(Guid.Empty);
            document.Complete();

            await documentWriteService.AddAsync(document, ct);

            await transactionWriteService.StageAddAsync(
                new CreateInventoryTransactionDto(
                    inventory.Id,
                    inventory.ProductId,
                    inventory.VariantId,
                    inventory.WarehouseId,
                    InventoryTransactionType.Adjustment,
                    delta,
                    adjustment.Entity.AvailableQuantity,
                    request.Reason.Trim()),
                ct);

            logger.Information(
                "Adjusted inventory {InventoryId} from {OldQuantity} to {NewQuantity} (delta: {Delta})",
                request.InventoryId, oldQuantity, request.NewQuantity, delta);
        }, ct: ct);

        return new AdjustStockResponse(adjustment!.Entity.AvailableQuantity);
    }
}
