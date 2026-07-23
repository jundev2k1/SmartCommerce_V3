using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.Warehouses;

namespace Inventory.Application.Features.Inventories.Events.OnProductVariationCreated;

/// <summary>
/// Every new ProductVariation needs a stock record before it can be tracked. Defaults new
/// inventory to the well-known "MAIN" warehouse (seeded by InventorySeeder) at zero stock - a
/// real warehouse assignment happens later via StockIn.
/// </summary>
public sealed class OnProductVariationCreatedHandler(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IWarehouseReadService warehouseReadService,
    IUnitOfWork unitOfWork,
    IAppLogger<OnProductVariationCreatedHandler> logger) : IInternalEventHandler<OnProductVariationCreatedEvent>
{
    private const string DefaultWarehouseCode = "MAIN";

    public async Task Handle(OnProductVariationCreatedEvent @event, CancellationToken ct = default)
    {
        var warehouse = await warehouseReadService.GetByCodeAsync(DefaultWarehouseCode, ct);
        if (warehouse is null)
        {
            logger.Warning(
                "Default warehouse {WarehouseCode} not found, skipping inventory initialization for ProductVariation {ProductVariationId}",
                DefaultWarehouseCode,
                @event.ProductVariationId);
            return;
        }

        // Idempotency safety net beyond the Inbox dedup - a redelivered/replayed message must
        // never create a second zero-stock row for the same (variation, warehouse) pair.
        var existing = await inventoryReadService.GetByVariationAndWarehouseAsync(@event.ProductVariationId, warehouse.Id, ct);
        if (existing is not null)
            return;

        var inventory = InventoryEntity.Create(
            Guid.CreateVersion7(),
            @event.ProductId,
            @event.ProductVariationId,
            warehouse.Id);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.AddAsync(inventory, ct);
        }, ct: ct);
    }
}
