using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Domain.Exceptions;

using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Application.Features.Inventories.DTOs;

namespace Inventory.Application.Features.Inventories.Commands.DeductStock;

public sealed class DeductStockHandler(
    IInventoryDocumentReadService documentReadService,
    IInventoryDocumentWriteService documentWriteService,
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IWarehouseReadService warehouseReadService,
    IUnitOfWork unitOfWork,
    IAppLogger<DeductStockHandler> logger) : ICommandHandler<DeductStockCommand, DeductStockResult>
{
    private const string MainWarehouseCode = "MAIN";
    private const int MaxConcurrencyRetries = 3;

    public async Task<DeductStockResult> Handle(DeductStockCommand request, CancellationToken ct = default)
    {
        var existing = await documentReadService.GetByNumberAsync(request.DeductionId.ToString(), ct);
        if (existing is not null)
        {
            logger.Information(
                "Stock deduction {DeductionId} already processed ({Status}), replaying result",
                request.DeductionId, existing.Status);
            return ToResult(existing);
        }

        var warehouse = await warehouseReadService.GetByCodeAsync(MainWarehouseCode, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Warehouse '{MainWarehouseCode}' is not configured.");

        for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            try
            {
                return await TryDeductAsync(request, warehouse.Id, ct);
            }
            catch (ConflictException) when (attempt < MaxConcurrencyRetries)
            {
                logger.Warning(
                    "Concurrent stock update detected while deducting {DeductionId} (attempt {Attempt}/{Max}), retrying",
                    request.DeductionId, attempt, MaxConcurrencyRetries);
            }
        }

        throw new ConflictException("Inventory is being updated concurrently. Please retry.");
    }

    private async Task<DeductStockResult> TryDeductAsync(DeductStockCommand request, Guid warehouseId, CancellationToken ct)
    {
        DeductStockResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var insufficient = new List<InsufficientStockItem>();
            var validated = new List<(DeductStockItem Item, Guid InventoryId, int ProductId, Guid ProductVariantId)>();

            foreach (var item in request.Items)
            {
                var inventory = await inventoryReadService.GetByVariationAndWarehouseAsync(item.ProductVariationId, warehouseId, ct);

                if (inventory is null)
                {
                    insufficient.Add(new InsufficientStockItem(item.ProductVariationId, item.Quantity, 0));
                    continue;
                }

                if (inventory.AvailableQuantity < item.Quantity)
                {
                    insufficient.Add(new InsufficientStockItem(item.ProductVariationId, item.Quantity, inventory.AvailableQuantity));
                    continue;
                }

                validated.Add((item, inventory.Id, inventory.ProductId, inventory.VariantId));
            }

            if (insufficient.Count > 0)
            {
                var failureDoc = InventoryDocument.Create(
                    request.DeductionId.ToString(),
                    InventoryDocumentType.Issue,
                    InventoryDocumentReason.StockOut,
                    sourceWarehouseId: warehouseId,
                    destinationWarehouseId: null);

                failureDoc.Cancel();
                await documentWriteService.AddAsync(failureDoc, ct);

                result = new DeductStockResult(false, "InsufficientStock", insufficient);
                return;
            }

            var reasonText = request.Reason is null ? "Order deduction" : $"Order deduction: {request.Reason}";
            var document = InventoryDocument.Create(
                request.DeductionId.ToString(),
                InventoryDocumentType.Issue,
                InventoryDocumentReason.StockOut,
                sourceWarehouseId: warehouseId,
                destinationWarehouseId: null,
                description: reasonText);

            foreach (var (item, inventoryId, productId, productVariantId) in validated)
            {
                var inv = await inventoryWriteService.DecreaseAsync(inventoryId, item.Quantity, ct);

                document.AddItem(
                    productId: productId,
                    productVariantId: productVariantId,
                    quantity: item.Quantity,
                    unitOfMeasure: "EA",
                    inventoryId: inventoryId,
                    description: reasonText);

                await transactionWriteService.StageAddAsync(
                    new CreateInventoryTransactionDto(
                        inv.Id,
                        inv.ProductId,
                        inv.VariantId,
                        inv.WarehouseId,
                        InventoryTransactionType.StockOut,
                        item.Quantity,
                        inv.AvailableQuantity,
                        reasonText),
                    ct);
            }

            document.Submit();
            document.Approve(Guid.Empty);
            document.Complete();

            await documentWriteService.AddAsync(document, ct);
            result = new DeductStockResult(true, null, []);
        }, ct: ct);

        return result!;
    }

    private static DeductStockResult ToResult(InventoryDocument document) => document.Status switch
    {
        InventoryDocumentStatus.Completed => new DeductStockResult(true, null, []),
        InventoryDocumentStatus.Cancelled => new DeductStockResult(false, "InsufficientStock", []),
        _ => new DeductStockResult(false, "ProcessingFailed", []),
    };
}
