using BuildingBlock.Application.Abstractions.Events;

using Order.Application.Abstractions.Repositories;

namespace Order.Application.Features.Catalog.Events.OnProductVariationCreated;

/// <summary>Keeps a local variation name/sku/price snapshot so CreateOrderHandler can validate and price requested variations without a synchronous call to Product Service.</summary>
public sealed class OnProductVariationCreatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogRepository catalogRepo) : IInternalEventHandler<OnProductVariationCreatedEvent>
{
    public async Task Handle(OnProductVariationCreatedEvent @event, CancellationToken ct = default)
    {
        var existing = await catalogRepo.GetByIdAsync(@event.ProductVariationId, ct);
        if (existing is not null)
        {
            await catalogRepo.UpdateAsync(@event.ProductVariationId, async (entry) =>
            {
                entry.UpdatePricing(@event.Sku, @event.Price, @event.Status);
                entry.UpdateProductName(@event.ProductName);
            }, ct);
        }
        else
        {
            var entry = OrderProductCatalog.Create(@event.ProductVariationId, @event.ProductId, @event.ProductName, @event.Sku, @event.Price, @event.Status);
            await catalogRepo.AddAsync(entry, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
