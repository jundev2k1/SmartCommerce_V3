using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;

namespace Order.Application.Features.Catalog.Events.OnProductUpdated;

/// <summary>A Product's Name is shared across every one of its variations, so this refreshes every OrderProductCatalog row for the product, not just one.</summary>
public sealed class OnProductUpdatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductUpdatedEvent>
{
    public async Task Handle(OnProductUpdatedEvent @event, CancellationToken ct = default)
    {
        await catalogWriteService.UpdateProductNameByProductIdAsync(@event.ProductId, @event.Name, ct);
        await uow.SaveChangesAsync(ct);
    }
}
