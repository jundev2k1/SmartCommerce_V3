using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Application.Authorization;
using SmartEcommerce.BuildingBlock.Infrastructure.Authorization;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using SmartEcommerce.Notification.Application.Features.NotificationChannels.Queries.ListNotificationChannels;

namespace SmartEcommerce.Notification.API.Endpoints.NotificationChannel;

/// <summary>
/// Channel rows are seeded by the system (see SmartEcommerce.Notification.Infrastructure) - there is no Create
/// endpoint here on purpose, matching NotificationChannel's domain rule that Admin may configure
/// but never create/delete a channel row.
/// </summary>
public sealed class ListChannels : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-channels", ListAsync)
            .WithTags("NotificationChannel")
            .RequirePermissions(Permissions.Notification.Manage)
            .WithName("ListNotificationChannels")
            .WithDisplayName("List Notification Channels API")
            .Produces<ApiResponse<IReadOnlyList<NotificationChannelSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new ListNotificationChannelsQuery(), ct);
        return Results.Ok(ApiResponse<IReadOnlyList<NotificationChannelSummaryResponse>>.Ok(response));
    }
}
