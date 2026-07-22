using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.Products.Commands.DeleteVariation;

namespace Product.API.Endpoints;

public sealed class DeleteVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Variation",
        "",
        "Removes a ProductVariation. The last remaining variation of a product can never be",
        "removed. Removing the current Default auto-promotes the remaining variation with the",
        "lowest DisplayOrder.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **variationId**: Unique identifier of the variation (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or variation not found",
        "- **400**: Cannot remove the last variation of a product",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{productId}/variations/{variationId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("DeleteVariation")
            .WithDisplayName("Delete Variation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteVariationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid variationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new DeleteVariationCommand(productId, variationId), ct);

        return Results.Ok(ApiResponse<DeleteVariationResponse>.Ok(response));
    }
}
