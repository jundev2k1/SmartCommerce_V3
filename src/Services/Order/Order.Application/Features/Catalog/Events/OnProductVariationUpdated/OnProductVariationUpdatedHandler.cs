using BuildingBlock.Application.Abstractions.Services;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;

namespace Order.Application.Features.Catalog.Events.OnProductVariationUpdated;

public sealed class OnProductVariationUpdatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogReadService catalogReadService,
    IOrderProductCatalogWriteService catalogWriteService,
    IAppLogger<OnProductVariationUpdatedHandler> logger) : IInternalEventHandler<OnProductVariationUpdatedEvent>
{
    public async Task Handle(OnProductVariationUpdatedEvent @event, CancellationToken ct = default)
    {
        var exists = await catalogReadService.ExistsAsync(@event.ProductVariationId, ct);
        if (!exists)
        {
            logger.Warning(
                "OrderProductCatalog entry not found for ProductVariationId {ProductVariationId}, skipping update",
                @event.ProductVariationId);
            return;
        }

        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.UpdatePricingAsync(
                    @event.ProductVariationId,
                    @event.Sku,
                    @event.Price,
                    @event.Status,
                    ct);
            },
            ct: ct);
    }
}
