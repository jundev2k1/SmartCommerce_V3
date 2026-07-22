using Mapster;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductTags.Queries.ListProductTags;

public sealed class ListProductTagsHandler(IProductTagRepository tagRepo)
    : IQueryHandler<ListProductTagsQuery, ListProductTagsResponse>
{
    public async Task<ListProductTagsResponse> Handle(ListProductTagsQuery request, CancellationToken ct = default)
    {
        var tags = await tagRepo.GetAllAsync(ct);

        return new ListProductTagsResponse(tags.Adapt<List<ProductTagItemResponse>>());
    }
}
