using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Domain.Exceptions;

using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.Warehouses;

namespace Inventory.Application.Features.Inventories.Commands.RestockStock;

public sealed class RestockStockHandler(
    IInventoryDocumentReadService documentReadService,
    IInventoryDocumentWriteService documentWriteService,
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionWriteService transactionWriteService,
    IWarehouseReadService warehouseReadService,
    IUnitOfWork unitOfWork,
    IAppLogger<RestockStockHandler> logger) : ICommandHandler<RestockStockCommand, RestockStockResult>
{
    private const string MainWarehouseCode = "MAIN";
    private const int MaxConcurrencyRetries = 3;

    public async Task<RestockStockResult> Handle(RestockStockCommand request, CancellationToken ct = default)
    {
        var document = await documentReadService.GetByNumberAsync(request.DeductionId.ToString(), ct);

        if (document is null || document.Status != InventoryDocumentStatus.Completed)
        {
            logger.Information(
                "RestockStock {DeductionId} is a no-op ({Status})",
                request.DeductionId, document?.Status.ToString() ?? "NeverDeducted");
            return new RestockStockResult(true);
        }

        var warehouse = await warehouseReadService.GetByCodeAsync(MainWarehouseCode, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Warehouse '{MainWarehouseCode}' is not configured.");

        var reasonText = request.Reason is null ? "Deduction reversal" : $"Deduction reversal: {request.Reason}";

        for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            try
            {
                await unitOfWork.ExecuteTransactionAsync(async () =>
                {
                    foreach (var item in document.Items)
                    {
                        var inventory = await inventoryReadService.GetByIdAsync(item.InventoryId!.Value, ct);
                        if (inventory is null)
                        {
                            logger.Warning(
                                "RestockStock {DeductionId}: no inventory row {InventoryId}, skipping",
                                request.DeductionId, item.InventoryId);
                            continue;
                        }

                        var inv = await inventoryWriteService.ReceiveStockAsync(inventory.Id, item.Quantity.Value, ct);

                        await transactionWriteService.StageAddAsync(
                            new CreateInventoryTransactionDto(
                                inv.Id,
                                inv.ProductId,
                                inv.VariantId,
                                inv.WarehouseId,
                                InventoryTransactionType.StockIn,
                                item.Quantity.Value,
                                inv.AvailableQuantity,
                                reasonText),
                            ct);
                    }
                }, ct: ct);

                return new RestockStockResult(true);
            }
            catch (ConflictException) when (attempt < MaxConcurrencyRetries)
            {
                logger.Warning(
                    "Concurrent stock update detected while restocking {DeductionId} (attempt {Attempt}/{Max}), retrying",
                    request.DeductionId, attempt, MaxConcurrencyRetries);
            }
        }

        throw new ConflictException("Inventory is being updated concurrently. Please retry.");
    }
}
