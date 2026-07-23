using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;

namespace Order.Application.Features.Catalog.Events.OnProductVariationDeleted;

public sealed class OnProductVariationDeletedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductVariationDeletedEvent>
{
    public async Task Handle(OnProductVariationDeletedEvent @event, CancellationToken ct = default)
    {
        await catalogWriteService.DeleteAsync(@event.ProductVariationId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
