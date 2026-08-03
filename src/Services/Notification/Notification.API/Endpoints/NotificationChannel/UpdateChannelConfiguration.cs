using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationChannels.Commands.UpdateNotificationChannelConfiguration;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationChannel;

public sealed class UpdateChannelConfiguration : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/notification-channels/{channelId}/configuration", UpdateConfigurationAsync)
            .WithTags("NotificationChannel")
            .RequireAuthorization(AuthorizationPoliciesConstant.RequireAdmin)
            .WithName("UpdateNotificationChannelConfiguration")
            .WithDisplayName("Update Notification Channel Configuration API")
            .WithDescription("Replaces a channel's runtime configuration (SMTP host, bot token, ...). Resets ValidationStatus to NotValidated.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> UpdateConfigurationAsync(
        [FromRoute] Guid channelId,
        [FromBody] UpdateChannelConfigurationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new UpdateNotificationChannelConfigurationCommand(channelId, request.ConfigJson), ct);
        return Results.Ok(ApiResponse<object>.Ok());
    }
}

public sealed record UpdateChannelConfigurationRequest(string ConfigJson);
