using Order.Application.Abstractions.Persistence.OrderProductCatalogs;

namespace Order.Application.Features.Catalog.Events.OnProductVariationCreated;

/// <summary>Keeps a local variation name/sku/price snapshot so CreateOrderHandler can validate and price requested variations without a synchronous call to Product Service.</summary>
public sealed class OnProductVariationCreatedHandler(
    IUnitOfWork uow,
    IOrderProductCatalogReadService catalogReadService,
    IOrderProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductVariationCreatedEvent>
{
    public async Task Handle(OnProductVariationCreatedEvent @event, CancellationToken ct = default)
    {
        // Check if exists product variation
        var isExist = await catalogReadService.ExistsAsync(@event.ProductVariationId, ct);

        // Handle upsert product variation snapshot
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                if (isExist)
                {
                    await catalogWriteService.UpdateVariationSnapshotAsync(
                        @event.ProductVariationId,
                        @event.ProductName,
                        @event.Sku,
                        @event.Price,
                        @event.Status,
                        ct);
                }
                else
                {
                    var entry = OrderProductCatalog.Create(
                        @event.ProductVariationId,
                        @event.ProductId,
                        @event.ProductName,
                        @event.Sku,
                        @event.Price,
                        @event.Status);
                    await catalogWriteService.CreateAsync(entry, ct);
                }
            },
            ct: ct);
    }
}
