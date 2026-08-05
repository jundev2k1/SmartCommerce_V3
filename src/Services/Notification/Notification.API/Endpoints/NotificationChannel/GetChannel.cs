using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationChannel;

public sealed class GetChannel : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-channels/{channelId}", GetAsync)
            .WithTags("NotificationChannel")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("GetNotificationChannel")
            .WithDisplayName("Get Notification Channel API")
            .Produces<ApiResponse<GetNotificationChannelResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationChannelQuery(channelId), ct);
        return Results.Ok(ApiResponse<GetNotificationChannelResponse>.Ok(response));
    }
}
