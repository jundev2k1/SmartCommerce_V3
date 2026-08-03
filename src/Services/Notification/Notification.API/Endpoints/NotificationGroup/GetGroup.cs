using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;
using BuildingBlock.SharedKernel.Constants;

using Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;

namespace Notification.API.Endpoints.NotificationGroup;

public sealed class GetGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-groups/{groupId}", GetAsync)
            .WithTags("NotificationGroup")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationGroup")
            .WithDisplayName("Get Notification Group API")
            .Produces<ApiResponse<GetNotificationGroupResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid groupId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationGroupQuery(groupId), ct);
        return Results.Ok(ApiResponse<GetNotificationGroupResponse>.Ok(response));
    }
}
