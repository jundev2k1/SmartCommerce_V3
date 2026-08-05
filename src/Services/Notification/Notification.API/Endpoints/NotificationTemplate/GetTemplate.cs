using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationTemplates.Queries.GetNotificationTemplate;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationTemplate;

public sealed class GetTemplate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-templates/{templateId}", GetAsync)
            .WithTags("NotificationTemplate")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("GetNotificationTemplate")
            .WithDisplayName("Get Notification Template API")
            .Produces<ApiResponse<GetNotificationTemplateResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid templateId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationTemplateQuery(templateId), ct);
        return Results.Ok(ApiResponse<GetNotificationTemplateResponse>.Ok(response));
    }
}
