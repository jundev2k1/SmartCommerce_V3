using SmartEcommerce.Product.Application.Features.Products.DTOs;

namespace SmartEcommerce.Product.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<ProductVariationInputDto> Variations,
    IReadOnlyCollection<Guid>? CategoryIds = null,
    IReadOnlyCollection<Guid>? TagIds = null) : ICommand<CreateProductResponse>;

public sealed record CreateProductResponse(Guid ProductId, Guid DefaultVariationId);
