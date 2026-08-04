using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;
using SmartEcommerce.Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

namespace SmartEcommerce.Product.API.Endpoints.ProductCategory;

public sealed record CreateProductCategoryRequest(
    string Code,
    string Name,
    string Description,
    string Note = "",
    Guid? ParentCategoryId = null);

public sealed class CreateProductCategoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Product Category",
        "",
        "Creates a new product category, optionally nested under a parent category.",
        "",
        "### Request Body",
        "- **Code**: Unique category code (required, must be unique)",
        "- **Name**: Category name (required)",
        "- **Description**: Category description",
        "- **ParentCategoryId**: Optional parent category (must already exist)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: ParentCategoryId does not exist",
        "- **409**: Code already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", Handle)
            .WithTags("ProductCategory")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("CreateProductCategory")
            .WithDisplayName("Create Product Category API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateProductCategoryResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateProductCategoryRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateProductCategoryCommand(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Note?.Trim() ?? string.Empty,
            request.ParentCategoryId);

        var response = await sender.Send(command, ct);

        return Results.Created($"/categories/{response.ProductCategoryId}",
            ApiResponse<CreateProductCategoryResponse>.Ok(response));
    }
}
