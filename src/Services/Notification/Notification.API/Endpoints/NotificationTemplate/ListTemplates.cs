using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationTemplates.Queries.ListNotificationTemplates;

namespace Notification.API.Endpoints.NotificationTemplate;

public sealed class ListTemplates : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-templates", ListAsync)
            .WithTags("NotificationTemplate")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationTemplates")
            .WithDisplayName("List Notification Templates API")
            .Produces<ApiResponse<PaginatedResult<NotificationTemplateSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] NotificationChannelType? channel,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationTemplatesQuery(
            channel,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationTemplateSummaryResponse>>.Ok(response));
    }
}
