using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.Products.Commands.UpdateVariation;

namespace Product.API.Endpoints;

public sealed record UpdateVariationRequest(
    string Sku,
    decimal Price,
    string Status,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null);

public sealed class UpdateVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Variation",
        "",
        "Updates a ProductVariation's details (Sku, Barcode, Price, Cost, Weight, Dimensions,",
        "Images, Status). Never touches DisplayOrder or IsDefault - use ReorderVariations /",
        "ChangeDefaultVariation for those.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **variationId**: Unique identifier of the variation (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or variation not found",
        "- **409**: Sku already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{productId}/variations/{variationId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("UpdateVariation")
            .WithDisplayName("Update Variation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateVariationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid variationId,
        [FromBody] UpdateVariationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateVariationCommand(
            productId,
            variationId,
            request.Sku.Trim(),
            request.Price,
            request.Status.Trim(),
            request.Barcode?.Trim(),
            request.Cost,
            request.Weight,
            request.DimensionsLength,
            request.DimensionsWidth,
            request.DimensionsHeight,
            request.Images);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateVariationResponse>.Ok(response));
    }
}
