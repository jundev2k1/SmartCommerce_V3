using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

using SmartEcommerce.Product.Application.Features.ProductTags.Commands.DeleteProductTag;

namespace SmartEcommerce.Product.API.Endpoints.ProductTag;

public sealed class DeleteProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Product Tag",
        "",
        "Deletes a product tag. Refuses if the tag is still assigned to any product.",
        "",
        "### Route Parameters",
        "- **tagId**: Unique identifier of the tag (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductTag not found",
        "- **409**: Tag is still assigned to products",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tags/{tagId}", Handle)
            .WithTags("ProductTag")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("DeleteProductTag")
            .WithDisplayName("Delete Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid tagId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new DeleteProductTagCommand(tagId), ct);

        return Results.Ok(ApiResponse<DeleteProductTagResponse>.Ok(response));
    }
}
