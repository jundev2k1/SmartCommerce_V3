using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Catalog.Events.OnProductDeleted;

public sealed class OnProductDeletedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogRepository catalogRepo) : IInternalEventHandler<OnProductDeletedEvent>
{
    public async Task Handle(OnProductDeletedEvent @event, CancellationToken ct = default)
    {
        await catalogRepo.DeleteByProductIdAsync(@event.ProductId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
