namespace Product.Application.Features.Products.Commands.UpdateProduct;

/// <summary>Product-level info only - never touches ProductVariation, see ProductVariation-specific commands for that.</summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    string Slug) : ICommand<UpdateProductResponse>;

public sealed record UpdateProductResponse;
