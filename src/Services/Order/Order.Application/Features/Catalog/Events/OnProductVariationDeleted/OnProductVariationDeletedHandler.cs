using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Catalog.Events.OnProductVariationDeleted;

public sealed class OnProductVariationDeletedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogRepository catalogRepo) : IInternalEventHandler<OnProductVariationDeletedEvent>
{
    public async Task Handle(OnProductVariationDeletedEvent @event, CancellationToken ct = default)
    {
        await catalogRepo.DeleteAsync(@event.ProductVariationId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
