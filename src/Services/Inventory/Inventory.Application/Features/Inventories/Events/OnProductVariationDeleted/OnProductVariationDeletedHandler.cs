using BuildingBlock.Application.Abstractions.Events;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Events.OnProductVariationDeleted;

/// <summary>A deleted variation no longer exists to hold stock against, so its inventory rows (across every warehouse) are removed with it.</summary>
public sealed class OnProductVariationDeletedHandler(
    IUnitOfWork uow,
    IInventoryRepository inventoryRepo) : IInternalEventHandler<OnProductVariationDeletedEvent>
{
    public async Task Handle(OnProductVariationDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await inventoryRepo.DeleteByVariationIdAsync(@event.ProductVariationId, ct);
        }, ct: ct);
    }
}
