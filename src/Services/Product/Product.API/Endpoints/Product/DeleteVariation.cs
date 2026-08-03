using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Product.Application.Features.Products.Commands.DeleteVariation;

namespace SmartEcommerce.Product.API.Endpoints.Product;

public sealed class DeleteVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Variation",
        "",
        "Removes a Variant. The last remaining variation of a product can never be",
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
            .WithTags("Product")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAdmin)
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
