using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.Products.Commands.AssignProductCategory;

namespace Product.API.Endpoints;

public sealed class AssignProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Assign Product Category",
        "",
        "Assigns a category to a product (many-to-many; idempotent).",
        "",
        "### Route Parameters",
        "- **productId**: Unique identifier of the product (required, must be valid GUID)",
        "- **categoryId**: Category to assign (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Product or category not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/{productId}/categories/{categoryId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("AssignProductCategory")
            .WithDisplayName("Assign Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AssignProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid productId,
        [FromRoute] Guid categoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new AssignProductCategoryCommand(productId, categoryId), ct);

        return Results.Ok(ApiResponse<AssignProductCategoryResponse>.Ok(response));
    }
}
