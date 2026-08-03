using SmartEcommerce.Product.Application.Features.Products.DTOs;

namespace SmartEcommerce.Product.Application.Features.Products.Commands.AddVariation;

public sealed record AddVariationCommand(
    Guid ProductId,
    ProductVariationInputDto VariationInput) : ICommand<AddVariationResponse>;

public sealed record AddVariationResponse(ProductVariationResponse Variation);
