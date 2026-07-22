using BuildingBlock.Application.Exceptions;

using Mapster;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductTags.Queries.GetProductTag;

public sealed class GetProductTagHandler(IProductTagRepository tagRepo)
    : IQueryHandler<GetProductTagQuery, GetProductTagResponse>
{
    public async Task<GetProductTagResponse> Handle(GetProductTagQuery request, CancellationToken ct = default)
    {
        var tag = await tagRepo.GetByIdAsync(request.ProductTagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.ProductTagId);

        return tag.Adapt<GetProductTagResponse>();
    }
}
