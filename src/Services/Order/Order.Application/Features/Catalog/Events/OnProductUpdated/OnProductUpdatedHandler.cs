using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Catalog.Events.OnProductUpdated;

/// <summary>A Product's Name is shared across every one of its variations, so this refreshes every OrderProductCatalog row for the product, not just one.</summary>
public sealed class OnProductUpdatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogRepository catalogRepo) : IInternalEventHandler<OnProductUpdatedEvent>
{
    public async Task Handle(OnProductUpdatedEvent @event, CancellationToken ct = default)
    {
        var entries = await catalogRepo.GetByProductIdAsync(@event.ProductId, ct);

        foreach (var entry in entries)
        {
            await catalogRepo.UpdateAsync(entry.Id, async (row) =>
            {
                row.UpdateProductName(@event.Name);
            }, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
