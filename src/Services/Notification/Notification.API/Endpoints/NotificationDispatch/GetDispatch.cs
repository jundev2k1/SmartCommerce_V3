using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;

using Notification.Application.Features.NotificationDispatches.Queries.GetNotificationDispatch;

namespace Notification.API.Endpoints.NotificationDispatch;

/// <summary>Ops visibility only - no Create endpoint. Dispatches are meant to be produced internally when a NotificationRule/NotificationCampaign executes (not implemented yet).</summary>
public sealed class GetDispatch : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-dispatches/{dispatchId}", GetAsync)
            .WithTags("NotificationDispatch")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationDispatch")
            .WithDisplayName("Get Notification Dispatch API")
            .Produces<ApiResponse<GetNotificationDispatchResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid dispatchId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationDispatchQuery(dispatchId), ct);
        return Results.Ok(ApiResponse<GetNotificationDispatchResponse>.Ok(response));
    }
}
