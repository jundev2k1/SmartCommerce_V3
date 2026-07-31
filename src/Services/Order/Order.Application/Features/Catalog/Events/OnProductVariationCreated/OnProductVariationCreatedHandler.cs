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
                        @event.ProductId,
                        @event.ProductVariationId,
                        @event.ProductName,
                        Sku.Create(@event.Sku),
                        Money.Create(@event.Price),
                        Enum.Parse<OrderProductCatalogStatus>(@event.Status),
                        ct);
                }
                else
                {
                    var entry = OrderProductCatalog.Create(
                        @event.ProductId,
                        @event.ProductVariationId,
                        @event.ProductName,
                        @event.VariationName,
                        Sku.Create(@event.Sku),
                        Money.Create(@event.Price),
                        Enum.Parse<OrderProductCatalogStatus>(@event.Status));
                    await catalogWriteService.CreateAsync(entry, ct);
                }
            },
            ct: ct);
    }
}
