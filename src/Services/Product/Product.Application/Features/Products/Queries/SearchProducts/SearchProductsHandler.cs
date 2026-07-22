using BuildingBlock.Application.Abstractions.Common;

using Product.Application.Abstractions.Search;

namespace Product.Application.Features.Products.Queries.SearchProducts;

public sealed class SearchProductsHandler(IProductSearchRepository searchRepo)
    : IQueryHandler<SearchProductsQuery, PaginatedResult<SearchProductsItemResponse>>
{
    public async Task<PaginatedResult<SearchProductsItemResponse>> Handle(
        SearchProductsQuery request, CancellationToken ct = default)
    {
        var criteria = new ProductSearchCriteria(
            request.Search, request.CategoryId, request.TagId, request.Status,
            request.SortBy, request.SortDescending, request.Page, request.PageSize);

        var (items, totalCount) = await searchRepo.SearchAsync(criteria, ct);

        var mapped = items
            .Select(d => new SearchProductsItemResponse(
                d.ProductId, d.Code, d.Name, d.Slug, d.Thumbnail, d.DefaultPrice,
                d.DefaultVariationId, d.DefaultVariationSku, d.CategoryIds, d.CategoryNames,
                d.TagIds, d.TagNames, d.Status, d.UpdatedAt))
            .ToList();

        return PaginatedResult<SearchProductsItemResponse>.Create(mapped, request.Page, request.PageSize, (int)totalCount);
    }
}
