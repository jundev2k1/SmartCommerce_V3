using Mapster;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductCategories.Queries.ListProductCategories;

public sealed class ListProductCategoriesHandler(IProductCategoryRepository categoryRepo)
    : IQueryHandler<ListProductCategoriesQuery, ListProductCategoriesResponse>
{
    public async Task<ListProductCategoriesResponse> Handle(ListProductCategoriesQuery request, CancellationToken ct = default)
    {
        var categories = await categoryRepo.GetAllAsync(ct);

        return new ListProductCategoriesResponse(categories.Adapt<List<ProductCategoryItemResponse>>());
    }
}
