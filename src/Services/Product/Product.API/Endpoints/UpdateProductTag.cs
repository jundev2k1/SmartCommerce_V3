using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Extensions;

using Product.Application.Features.ProductTags.Commands.UpdateProductTag;

namespace Product.API.Endpoints;

public sealed record UpdateProductTagRequest(string Name);

public sealed class UpdateProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Product Tag",
        "",
        "Renames a product tag.",
        "",
        "### Route Parameters",
        "- **tagId**: Unique identifier of the tag (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductTag not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{tagId}", Handle)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("UpdateProductTag")
            .WithDisplayName("Update Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid tagId,
        [FromBody] UpdateProductTagRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new UpdateProductTagCommand(tagId, request.Name.Trim()), ct);

        return Results.Ok(ApiResponse<UpdateProductTagResponse>.Ok(response));
    }
}
