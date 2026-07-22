using Product.Application.Abstractions.Repositories;
using Product.Application.Abstractions.Search;
using Product.Application.Features.Products.Search;

namespace Product.Application.Features.Products.Commands.RebuildProductSearchIndex;

/// <summary>
/// PostgreSQL -&gt; Projection Builder -&gt; Bulk Index -&gt; Elasticsearch. Reuses the exact same
/// ProjectionBuilder/IProductSearchIndexer the live sync path uses (see
/// OnProductSearchSyncRequiredHandler) - future schema changes only touch the Projection
/// Builder, not this orchestration. See docs/reference/search.md.
/// </summary>
public sealed class RebuildProductSearchIndexHandler(
    IProductRepository productRepo,
    ProductSearchProjectionBuilder projectionBuilder,
    IProductSearchIndexer searchIndexer) : ICommandHandler<RebuildProductSearchIndexCommand, RebuildProductSearchIndexResponse>
{
    private const int BatchSize = 200;

    public async Task<RebuildProductSearchIndexResponse> Handle(
        RebuildProductSearchIndexCommand request, CancellationToken ct = default)
    {
        await searchIndexer.RecreateIndexAsync(ct);

        var indexed = 0;
        var skip = 0;
        IReadOnlyList<ProductEntity> batch;

        do
        {
            batch = await productRepo.GetAllAsync(skip, BatchSize, ct);
            if (batch.Count == 0)
                break;

            var documents = await projectionBuilder.BuildManyAsync(batch, ct);
            await searchIndexer.BulkIndexAsync(documents, ct);

            indexed += batch.Count;
            skip += BatchSize;
        } while (batch.Count == BatchSize);

        return new RebuildProductSearchIndexResponse(indexed);
    }
}
