using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Infrastructure.Authorization;

using MediatR;

using Microsoft.AspNetCore.Authorization;

using Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;
using Notification.Application.Features.UserNotifications.DTOs;
using Notification.Infrastructure.SignalR.Groups;

namespace Notification.Infrastructure.SignalR.Hubs.Global;

public interface IGlobalHubBase : IAppHub
{
    Task ReceiveNotification(NotificationDto message);
}

public interface IGlobalHubClient
    : IAdminSiteActions, IClientSiteActions, IGlobalHubBase
{
}

[Authorize(Policy = AuthorizationPolicies.RequireAuthenticated)]
public partial class GlobalHub(
    ISender sender,
    IAppLogger<GlobalHub> logger) : HubBase<IGlobalHubClient>
{
    public const string Path = "/hubs/global";

    public override async Task OnConnectedAsync()
    {
        logger.Information($"User connected ({nameof(GlobalHub)}): {this.UserId}");

        var roleList = this.Roles;
        if (roleList.Contains(AppRole.Root))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Root(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRole.Root));
        }

        if (roleList.Contains(AppRole.Admin))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Admin(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRole.Admin));
        }

        var isUser = roleList.Contains(AppRole.User);
        if (isUser)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Member(this.UserId));
            await Groups.AddToGroupAsync(Context.ConnectionId, ActorGroups.Broadcast(AppRole.User));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.Information($"User disconnected: {Context.UserIdentifier}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        var command = new MarkUserNotificationAsReadCommand(notificationId);
        await sender.Send(command);
    }
}
