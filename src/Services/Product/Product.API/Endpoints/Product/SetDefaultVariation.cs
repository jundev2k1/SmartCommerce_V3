using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Product.Application.Features.Products.Commands.SetDefaultVariation;

namespace SmartEcommerce.Product.API.Endpoints.Product;

public sealed class SetDefaultVariationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Change Default Variation",
        "",
        "Switches which variation is the Default. No-op if it already is.",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **variationId**: Variation to make the new Default (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or variation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/variations/{variationId}/default", Handle)
            .WithTags("Product")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("SetDefaultVariation")
            .WithDisplayName("Change Default Variation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<SetDefaultVariationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid variationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new SetDefaultVariationCommand(productId, variationId), ct);

        return Results.Ok(ApiResponse<SetDefaultVariationResponse>.Ok(response));
    }
}
