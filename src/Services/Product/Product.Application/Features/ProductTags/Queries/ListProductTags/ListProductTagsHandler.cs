using Mapster;

using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Application.Features.ProductTags.Queries.ListProductTags;

public sealed class ListProductTagsHandler(IProductTagReadService tagReadService)
    : IQueryHandler<ListProductTagsQuery, ListProductTagsResponse>
{
    public async Task<ListProductTagsResponse> Handle(ListProductTagsQuery request, CancellationToken ct = default)
    {
        var tags = await tagReadService.GetAllAsync(ct);

        return new ListProductTagsResponse(tags.Adapt<List<ProductTagItemResponse>>());
    }
}
