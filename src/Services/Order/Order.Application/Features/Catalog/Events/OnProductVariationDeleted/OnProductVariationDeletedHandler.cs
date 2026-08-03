using SmartEcommerce.Order.Application.Abstractions.Persistence.ProductCatalogs;

namespace SmartEcommerce.Order.Application.Features.Catalog.Events.OnProductVariationDeleted;

public sealed class OnProductVariationDeletedHandler(
    IUnitOfWork uow,
    IProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductVariationDeletedEvent>
{
    public async Task Handle(OnProductVariationDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.DeleteAsync(
                    @event.ProductId,
                    @event.ProductVariationId,
                    ct);
            },
            ct: ct);
    }
}
