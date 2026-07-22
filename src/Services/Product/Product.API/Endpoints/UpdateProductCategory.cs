using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.ProductCategories.Commands.UpdateProductCategory;

namespace Product.API.Endpoints;

public sealed record UpdateProductCategoryRequest(
    string Name,
    string Description,
    Guid? ParentCategoryId = null);

public sealed class UpdateProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Product Category",
        "",
        "Updates product category details, including moving it under a different parent.",
        "",
        "### Route Parameters",
        "- **categoryId**: Unique identifier of the category (required, must be valid GUID)",
        "",
        "### Request Body",
        "- **Name**: Category name (required)",
        "- **Description**: Category description",
        "- **ParentCategoryId**: New parent, or null to move to root",
        "",
        "### Error Responses",
        "- **404**: ProductCategory or ParentCategoryId not found",
        "- **400**: Invalid categoryId format or validation failed",
        "- **409**: Would create a cycle (moving a category under one of its own descendants)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{categoryId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("UpdateProductCategory")
            .WithDisplayName("Update Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateProductCategoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid categoryId,
        [FromBody] UpdateProductCategoryRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateProductCategoryCommand(
            categoryId,
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.ParentCategoryId);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateProductCategoryResponse>.Ok(response));
    }
}
