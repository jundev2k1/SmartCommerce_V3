using BuildingBlock.Application.Exceptions;

using Mapster;

using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Application.Features.ProductTags.Queries.GetProductTag;

public sealed class GetProductTagHandler(IProductTagReadService tagReadService)
    : IQueryHandler<GetProductTagQuery, GetProductTagResponse>
{
    public async Task<GetProductTagResponse> Handle(GetProductTagQuery request, CancellationToken ct = default)
    {
        var tag = await tagReadService.GetByIdAsync(request.ProductTagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        return tag.Adapt<GetProductTagResponse>();
    }
}
