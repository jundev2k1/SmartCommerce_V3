using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Infrastructure.Authorization;

using Notification.Application.Features.NotificationChannels.Commands.DisableNotificationChannel;
using Notification.Application.Features.NotificationChannels.Commands.EnableNotificationChannel;
using Notification.Application.Features.NotificationChannels.Commands.UpdateNotificationChannelConfiguration;
using Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;
using Notification.Application.Features.NotificationChannels.Queries.ListNotificationChannels;

namespace Notification.API.Endpoints;

/// <summary>
/// Channel rows are seeded by the system (see Notification.Infrastructure) - there is no Create
/// endpoint here on purpose, matching NotificationChannel's domain rule that Admin may configure
/// but never create/delete a channel row.
/// </summary>
public sealed class NotificationChannelEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-channels/{channelId}", GetAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("GetNotificationChannel")
            .WithDisplayName("Get Notification Channel API")
            .Produces<ApiResponse<GetNotificationChannelResponse>>(StatusCodes.Status200OK);

        app.MapGet("/notification-channels", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("ListNotificationChannels")
            .WithDisplayName("List Notification Channels API")
            .Produces<ApiResponse<IReadOnlyList<NotificationChannelSummaryResponse>>>(StatusCodes.Status200OK);

        app.MapPut("/notification-channels/{channelId}/configuration", UpdateConfigurationAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("UpdateNotificationChannelConfiguration")
            .WithDisplayName("Update Notification Channel Configuration API")
            .WithDescription("Replaces a channel's runtime configuration (SMTP host, bot token, ...). Resets ValidationStatus to NotValidated.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        app.MapPost("/notification-channels/{channelId}/enable", EnableAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("EnableNotificationChannel")
            .WithDisplayName("Enable Notification Channel API")
            .WithDescription("Enables a channel. Requires the configuration to already be Valid (see RecordValidationResult).")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        app.MapPost("/notification-channels/{channelId}/disable", DisableAsync)
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .WithName("DisableNotificationChannel")
            .WithDisplayName("Disable Notification Channel API")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationChannelQuery(channelId), ct);
        return Results.Ok(ApiResponse<GetNotificationChannelResponse>.Ok(response));
    }

    private static async Task<IResult> ListAsync(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new ListNotificationChannelsQuery(), ct);
        return Results.Ok(ApiResponse<IReadOnlyList<NotificationChannelSummaryResponse>>.Ok(response));
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

    private static async Task<IResult> EnableAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new EnableNotificationChannelCommand(channelId), ct);
        return Results.Ok(ApiResponse<object>.Ok());
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

public sealed record UpdateChannelConfigurationRequest(string ConfigJson);
