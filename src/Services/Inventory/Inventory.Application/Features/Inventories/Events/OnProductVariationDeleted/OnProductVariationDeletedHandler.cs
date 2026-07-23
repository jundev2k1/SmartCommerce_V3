using BuildingBlock.Application.Abstractions.Events;

using Inventory.Application.Abstractions.Persistence.Inventories;

namespace Inventory.Application.Features.Inventories.Events.OnProductVariationDeleted;

/// <summary>A deleted variation no longer exists to hold stock against, so its inventory rows (across every warehouse) are removed with it.</summary>
public sealed class OnProductVariationDeletedHandler(
    IInventoryWriteService inventoryWriteService,
    IUnitOfWork unitOfWork) : IInternalEventHandler<OnProductVariationDeletedEvent>
{
    public async Task Handle(OnProductVariationDeletedEvent @event, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.DeleteByVariationIdAsync(@event.ProductVariationId, ct);
        }, ct: ct);
    }
}
