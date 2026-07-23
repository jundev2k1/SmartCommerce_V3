using Order.Application.Abstractions.Persistence.OrderProductCatalogs;

namespace Order.Application.Features.Catalog.Events.OnProductDeleted;

public sealed class OnProductDeletedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductDeletedEvent>
{
    public async Task Handle(OnProductDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.DeleteByProductIdAsync(@event.ProductId, ct);
            },
            ct: ct);
    }
}
