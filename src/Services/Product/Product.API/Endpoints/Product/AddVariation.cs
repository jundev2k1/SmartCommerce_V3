using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.Products.Commands.AddVariation;
using Product.Application.Features.Products.DTOs;

namespace Product.API.Endpoints.Product;

public sealed record AddVariationRequest(
    string Sku,
    string Name,
    decimal Price,
    bool MakeDefault = false,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null);

public sealed class AddVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Add Variation",
        "",
        "Adds a new ProductVariation to an existing product. DisplayOrder is assigned",
        "automatically (after the current highest). Set MakeDefault to switch the Default over",
        "to the new variation atomically.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product not found",
        "- **409**: Sku already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/variations", Handle)
            .WithTags("Product")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("AddVariation")
            .WithDisplayName("Add Variation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AddVariationResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromBody] AddVariationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new AddVariationCommand(
            productId,
            new ProductVariationInputDto(
                request.Sku.Trim(),
                request.Name.Trim(),
                request.Price,
                request.MakeDefault,
                request.Barcode?.Trim(),
                request.Cost,
                request.Weight,
                request.DimensionsLength,
                request.DimensionsWidth,
                request.DimensionsHeight,
                request.Images));

        var response = await sender.Send(command, ct);

        return Results.Created($"/products/{productId}/variations/{response.Variation.Id}",
            ApiResponse<AddVariationResponse>.Ok(response));
    }
}
