using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Queries.GetProduct;

public sealed class GetProductHandler(IProductRepository productRepo)
    : IQueryHandler<GetProductQuery, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductQuery request, CancellationToken ct = default)
    {
        var product = await productRepo.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        return new GetProductResponse(
            product.Id,
            product.Code.Value,
            product.Name,
            product.Description,
            product.Slug.Value,
            [.. product.CategoryMappings.Select(m => m.CategoryId)],
            [.. product.TagMappings.Select(m => m.TagId)],
            [.. product.Variations.Select(ProductVariationResponse.From)],
            product.CreatedAt,
            product.UpdatedAt);
    }
}
