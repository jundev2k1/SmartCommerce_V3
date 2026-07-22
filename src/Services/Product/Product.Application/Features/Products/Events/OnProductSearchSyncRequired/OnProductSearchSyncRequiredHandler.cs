using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;

using Product.Application.Abstractions.Repositories;
using Product.Application.Abstractions.Search;
using Product.Application.Features.Products.Search;

namespace Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

/// <summary>The Search Consumer's reaction: rebuild the document from current Postgres state and upsert it. See docs/reference/search.md.</summary>
public sealed class OnProductSearchSyncRequiredHandler(
    IProductRepository productRepo,
    ProductSearchProjectionBuilder projectionBuilder,
    IProductSearchIndexer searchIndexer,
    IAppLogger<OnProductSearchSyncRequiredHandler> logger) : IInternalEventHandler<OnProductSearchSyncRequiredEvent>
{
    public async Task Handle(OnProductSearchSyncRequiredEvent @event, CancellationToken ct = default)
    {
        var product = await productRepo.GetByIdAsync(@event.ProductId, ct);
        if (product is null)
        {
            logger.Warning("Product {ProductId} no longer exists - skipping search sync", @event.ProductId);
            return;
        }

        var document = await projectionBuilder.BuildAsync(product, ct);
        await searchIndexer.IndexAsync(document, ct);
    }
}
