using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationDispatches.Queries.GetNotificationDispatch;
using Notification.Application.Features.NotificationDispatches.Queries.ListNotificationDispatches;

namespace Notification.API.Endpoints;

/// <summary>Ops visibility only - no Create endpoint. Dispatches are meant to be produced internally when a NotificationRule/NotificationCampaign executes (not implemented yet).</summary>
public sealed class NotificationDispatchEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-dispatches/{dispatchId}", GetAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationDispatch")
            .WithDisplayName("Get Notification Dispatch API")
            .Produces<ApiResponse<GetNotificationDispatchResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-dispatches", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationDispatches")
            .WithDisplayName("List Notification Dispatches API")
            .Produces<ApiResponse<PaginatedResult<NotificationDispatchSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid dispatchId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationDispatchQuery(dispatchId), ct);
        return Results.Ok(ApiResponse<GetNotificationDispatchResponse>.Ok(response));
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] DispatchStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationDispatchesQuery(
            status,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationDispatchSummaryResponse>>.Ok(response));
    }
}
