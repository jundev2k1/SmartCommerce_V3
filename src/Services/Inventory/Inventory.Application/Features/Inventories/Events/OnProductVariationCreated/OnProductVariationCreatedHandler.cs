using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Events.OnProductVariationCreated;

/// <summary>
/// Every new ProductVariation needs a stock record before it can be tracked. Defaults new
/// inventory to the well-known "MAIN" warehouse (seeded by InventorySeeder) at zero stock - a
/// real warehouse assignment happens later via StockIn.
/// </summary>
public sealed class OnProductVariationCreatedHandler(
    IUnitOfWork uow,
    IInventoryRepository inventoryRepo,
    IWarehouseRepository warehouseRepo,
    IAppLogger<OnProductVariationCreatedHandler> logger) : IInternalEventHandler<OnProductVariationCreatedEvent>
{
    private const string DefaultWarehouseCode = "MAIN";

    public async Task Handle(OnProductVariationCreatedEvent @event, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepo.GetByCodeAsync(DefaultWarehouseCode, ct);
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
        var existing = await inventoryRepo.GetByVariationAndWarehouseAsync(@event.ProductVariationId, warehouse.Id, ct);
        if (existing is not null)
            return;

        await uow.ExecuteTransactionAsync(async () =>
        {
            var inventory = InventoryEntity.Create(
                Guid.CreateVersion7(),
                @event.ProductId,
                @event.ProductVariationId,
                warehouse.Id);
            await inventoryRepo.AddAsync(inventory, ct);
        }, ct: ct);
    }
}
