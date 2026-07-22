using BuildingBlock.Application.Exceptions;

using Mapster;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductCategories.Queries.GetProductCategory;

public sealed class GetProductCategoryHandler(IProductCategoryRepository categoryRepo)
    : IQueryHandler<GetProductCategoryQuery, GetProductCategoryResponse>
{
    public async Task<GetProductCategoryResponse> Handle(GetProductCategoryQuery request, CancellationToken ct = default)
    {
        var category = await categoryRepo.GetByIdAsync(request.ProductCategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.ProductCategoryId);

        // ChildCategoryIds comes from a separate repository call, not the entity itself - map the
        // entity fields, then splice in the composed field via `with` (record non-destructive mutation).
        var childIds = await categoryRepo.GetChildIdsAsync(request.ProductCategoryId, ct);

        return category.Adapt<GetProductCategoryResponse>() with { ChildCategoryIds = childIds };
    }
}
