using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed class StockOutHandler(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryDocumentWriteService documentWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IUnitOfWork unitOfWork,
    IAppLogger<StockOutHandler> logger) : ICommandHandler<StockOutCommand, StockOutResponse>
{
    public async Task<StockOutResponse> Handle(StockOutCommand request, CancellationToken ct = default)
    {
        var inventory = await inventoryReadService.GetByIdAsync(request.InventoryId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Inventory {request.InventoryId} not found.");

        InventoryStock? updated = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            updated = await inventoryWriteService.DeductStockAsync(request.InventoryId, request.Quantity, ct);

            var document = InventoryDocument.Create(
                number: Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant(),
                type: InventoryDocumentType.Issue,
                reason: InventoryDocumentReason.Sale,
                sourceWarehouseId: inventory.WarehouseId,
                destinationWarehouseId: null,
                description: request.Reason.Trim());

            document.AddItem(
                productId: inventory.ProductId,
                productVariantId: inventory.VariantId,
                quantity: request.Quantity,
                unitOfMeasure: "EA",
                inventoryId: inventory.Id,
                description: request.Reason.Trim());

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
                    InventoryTransactionType.StockOut,
                    request.Quantity,
                    updated.AvailableQuantity,
                    request.Reason.Trim()),
                ct);

            logger.Information(
                "Issued {Quantity} units from inventory {InventoryId}, remaining: {RemainingQuantity}",
                request.Quantity, request.InventoryId, updated.AvailableQuantity);
        }, ct: ct);

        return new StockOutResponse(updated!.AvailableQuantity);
    }
}
