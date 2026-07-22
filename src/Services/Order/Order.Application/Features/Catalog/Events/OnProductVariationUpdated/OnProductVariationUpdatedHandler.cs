using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Catalog.Events.OnProductVariationUpdated;

public sealed class OnProductVariationUpdatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogRepository catalogRepo,
    IAppLogger<OnProductVariationUpdatedHandler> logger) : IInternalEventHandler<OnProductVariationUpdatedEvent>
{
    public async Task Handle(OnProductVariationUpdatedEvent @event, CancellationToken ct = default)
    {
        var existing = await catalogRepo.GetByIdAsync(@event.ProductVariationId, ct);
        if (existing is null)
        {
            logger.Warning(
                "OrderProductCatalog entry not found for ProductVariationId {ProductVariationId}, skipping update",
                @event.ProductVariationId);
            return;
        }

        await catalogRepo.UpdateAsync(@event.ProductVariationId, async (entry) =>
        {
            entry.UpdatePricing(@event.Sku, @event.Price, @event.Status);
        }, ct);

        await uow.SaveChangesAsync(ct);
    }
}
