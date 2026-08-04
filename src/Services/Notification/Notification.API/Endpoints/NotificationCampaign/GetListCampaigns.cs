using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationCampaigns.Queries.ListNotificationCampaigns;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationCampaign;

public sealed class GetListCampaigns : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-campaigns", HandleAsync)
            .WithTags("NotificationCampaign")
            .RequirePermissions(Permissions.Notification.Manage)
            .WithName("ListNotificationCampaigns")
            .WithDisplayName("List Notification Campaigns API")
            .Produces<ApiResponse<PaginatedResult<NotificationCampaignSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] CampaignStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationCampaignsQuery(
            status,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationCampaignSummaryResponse>>.Ok(response));
    }
}
