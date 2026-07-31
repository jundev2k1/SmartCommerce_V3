using System.Text.Json;

using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Exceptions;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.StockDeductions;
using Inventory.Application.Abstractions.Persistence.Warehouses;

namespace Inventory.Application.Features.Inventories.Commands.DeductStock;

/// <summary>
/// Validates every requested item against current stock, then deducts all-or-nothing inside one
/// transaction. Idempotent via StockDeduction (keyed by DeductionId) - a retried call with the
/// same id replays the original outcome instead of decrementing twice. Retries a bounded number
/// of times on ConflictException (EfUnitOfWork's translation of a Postgres xmin conflict - see
/// InventoryConfig) since a concurrent deduction against the same row invalidates this attempt's
/// read and must re-validate against fresh quantities, not blindly retry the same numbers.
/// </summary>
public sealed class DeductStockHandler(
    IStockDeductionReadService deductionReadService,
    IStockDeductionWriteService deductionWriteService,
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
        var existing = await deductionReadService.GetByIdAsync(request.DeductionId, ct);
        if (existing is not null)
        {
            logger.Information("DeductStock {DeductionId} already processed ({Status}), replaying stored result", request.DeductionId, existing.Status);
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
            var validated = new List<(DeductStockItem Item, Guid InventoryId)>();

            foreach (var item in request.Items)
            {
                var inventory = await inventoryReadService.GetByVariationAndWarehouseAsync(item.ProductVariationId, warehouseId, ct);

                if (inventory is null)
                {
                    insufficient.Add(new InsufficientStockItem(item.ProductVariationId, item.Quantity, 0));
                    continue;
                }

                if (inventory.Quantity < item.Quantity)
                {
                    insufficient.Add(new InsufficientStockItem(item.ProductVariationId, item.Quantity, inventory.Quantity));
                    continue;
                }

                validated.Add((item, inventory.Id));
            }

            if (insufficient.Count > 0)
            {
                await deductionWriteService.StageFailedAsync(request.DeductionId, "InsufficientStock", request.Reason, ct);

                result = new DeductStockResult(false, "InsufficientStock", insufficient);
                return;
            }

            var reasonText = request.Reason is null ? "Order deduction" : $"Order deduction: {request.Reason}";

            foreach (var (item, inventoryId) in validated)
            {
                var inv = await inventoryWriteService.DecreaseAsync(inventoryId, item.Quantity, ct);

                await transactionWriteService.StageAddAsync(
                    new CreateInventoryTransactionRequest(
                        inv.Id,
                        inv.ProductId,
                        inv.VariationId,
                        inv.WarehouseId,
                        InventoryTransactionType.StockOut,
                        item.Quantity,
                        inv.Quantity,
                        reasonText),
                    ct);
            }

            var itemsJson = JsonSerializer.Serialize(request.Items);
            await deductionWriteService.StageSucceededAsync(request.DeductionId, itemsJson, request.Reason, ct);

            result = new DeductStockResult(true, null, []);
        }, ct: ct);

        return result!;
    }

    private static DeductStockResult ToResult(StockDeduction deduction) => deduction.Status switch
    {
        StockDeductionStatus.Succeeded or StockDeductionStatus.Reversed => new DeductStockResult(true, null, []),
        _ => new DeductStockResult(false, deduction.FailureCode ?? "InsufficientStock", []),
    };
}
