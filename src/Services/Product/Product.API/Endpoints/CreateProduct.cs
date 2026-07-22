using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.Products.Commands.CreateProduct;

namespace Product.API.Endpoints;

public sealed record CreateProductVariationRequest(
    string Sku,
    decimal Price,
    bool IsDefault = false,
    string? Barcode = null,
    decimal? Cost = null,
    decimal? Weight = null,
    decimal? DimensionsLength = null,
    decimal? DimensionsWidth = null,
    decimal? DimensionsHeight = null,
    IReadOnlyCollection<string>? Images = null);

public sealed record CreateProductRequest(
    string Code,
    string Name,
    string Description,
    string Slug,
    IReadOnlyCollection<CreateProductVariationRequest> Variations,
    IReadOnlyCollection<Guid>? CategoryIds = null,
    IReadOnlyCollection<Guid>? TagIds = null);

public sealed class CreateProductEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Product",
        "",
        "Creates a new product together with all of its initial variations in a single request -",
        "creating a Product is aggregate initialization, so this is the one place multiple",
        "variations may be submitted at once. After creation, use the dedicated ProductVariation",
        "APIs to add/update/remove/reorder variations.",
        "",
        "### Request Body",
        "- **Code**: Unique shared product code (required, must be unique)",
        "- **Name**: Product name (required)",
        "- **Description**: Product description",
        "- **Slug**: Unique URL slug (required, must be unique)",
        "- **Variations**: At least one variation (required). If none is marked IsDefault, the first is used.",
        "- **CategoryIds** / **TagIds**: Optional category/tag assignments (must already exist)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: A referenced CategoryId/TagId does not exist",
        "- **409**: Code, Slug, or a variation Sku already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("CreateProduct")
            .WithDisplayName("Create Product API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateProductResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateProductRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateProductCommand(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Slug.Trim(),
            [.. request.Variations.Select(v => new CreateProductVariationInput(
                v.Sku.Trim(),
                v.Price,
                v.IsDefault,
                v.Barcode?.Trim(),
                v.Cost,
                v.Weight,
                v.DimensionsLength,
                v.DimensionsWidth,
                v.DimensionsHeight,
                v.Images))],
            request.CategoryIds,
            request.TagIds);

        var response = await sender.Send(command, ct);

        return Results.Created($"/products/{response.ProductId}",
            ApiResponse<CreateProductResponse>.Ok(response));
    }
}
