using BuildingBlock.Application.Abstractions.Events;

using Inventory.Application.Abstractions.Repositories;

namespace Inventory.Application.Features.Inventories.Events.OnProductDeleted;

/// <summary>
/// Whole-product deletion is an EF cascade over the owned ProductVariation rows (see
/// Product.RemoveVariation vs. a full aggregate delete), so no per-variation Deleted event fires
/// for each one - this handler is the fallback that cleans up every inventory row for the
/// product in one pass.
/// </summary>
public sealed class OnProductDeletedHandler(
    IUnitOfWork uow,
    IInventoryRepository inventoryRepo) : IInternalEventHandler<OnProductDeletedEvent>
{
    public async Task Handle(OnProductDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            await inventoryRepo.DeleteByProductIdAsync(@event.ProductId, ct);
        }, ct: ct);
    }
}
