using System.Text.Json;

using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Exceptions;

using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.StockDeductions;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Application.Features.Inventories.Commands.DeductStock;

namespace Inventory.Application.Features.Inventories.Commands.RestockStock;

public sealed class RestockStockHandler(
    IStockDeductionReadService deductionReadService,
    IStockDeductionWriteService deductionWriteService,
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
        var deduction = await deductionReadService.GetByIdAsync(request.DeductionId, ct);

        // Nothing to reverse: never deducted, already reversed, or the original attempt failed.
        // A compensating action must never itself become a blocking failure - see
        // docs/reference/create-order-saga.md's compensation/idempotency notes.
        if (deduction is null || deduction.Status != StockDeductionStatus.Succeeded)
        {
            logger.Information(
                "RestockStock {DeductionId} is a no-op ({Status})",
                request.DeductionId, deduction?.Status.ToString() ?? "NeverDeducted");
            return new RestockStockResult(true);
        }

        var warehouse = await warehouseReadService.GetByCodeAsync(MainWarehouseCode, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Warehouse '{MainWarehouseCode}' is not configured.");

        var items = JsonSerializer.Deserialize<List<DeductStockItem>>(deduction.ItemsJson) ?? [];
        var reasonText = request.Reason is null ? "Deduction reversal" : $"Deduction reversal: {request.Reason}";

        for (var attempt = 1; attempt <= MaxConcurrencyRetries; attempt++)
        {
            try
            {
                await unitOfWork.ExecuteTransactionAsync(async () =>
                {
                    foreach (var item in items)
                    {
                        var inventory = await inventoryReadService.GetByVariationAndWarehouseAsync(item.ProductVariationId, warehouse.Id, ct);
                        if (inventory is null)
                        {
                            logger.Warning(
                                "RestockStock {DeductionId}: no inventory row for variation {VariationId}, skipping",
                                request.DeductionId, item.ProductVariationId);
                            continue;
                        }

                        await inventoryWriteService.StageUpdateAsync(inventory.Id, async inv =>
                        {
                            inv.Increase(item.Quantity);

                            await transactionWriteService.StageAddAsync(
                                InventoryTransaction.Create(
                                    Guid.CreateVersion7(),
                                    inv.Id,
                                    inv.ProductId,
                                    inv.ProductVariationId,
                                    inv.WarehouseId,
                                    InventoryTransactionType.StockIn,
                                    item.Quantity,
                                    inv.Quantity,
                                    reasonText),
                                ct);
                        }, ct);
                    }

                    await deductionWriteService.StageUpdateAsync(request.DeductionId, async d =>
                    {
                        d.MarkReversed();
                        await Task.CompletedTask;
                    }, ct);
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
