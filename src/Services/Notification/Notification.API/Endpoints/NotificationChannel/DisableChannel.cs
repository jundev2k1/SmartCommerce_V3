using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationChannels.Commands.DisableNotificationChannel;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationChannel;

public sealed class DisableChannel : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-channels/{channelId}/disable", DisableAsync)
            .WithTags("NotificationChannel")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAdmin)
            .WithName("DisableNotificationChannel")
            .WithDisplayName("Disable Notification Channel API")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> DisableAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new DisableNotificationChannelCommand(channelId), ct);
        return Results.Ok(ApiResponse<object>.Ok());
    }
}
