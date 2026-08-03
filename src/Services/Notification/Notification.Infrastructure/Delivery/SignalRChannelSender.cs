using System.Text.Json;

using SmartEcommerce.Notification.Application.Abstractions.Services;
using SmartEcommerce.Notification.Application.Features.NotificationDispatches.DTOs;
using SmartEcommerce.Notification.Application.Features.UserNotifications.DTOs;
using SmartEcommerce.Notification.Domain.Entities;
using SmartEcommerce.Notification.Domain.Enums;
using SmartEcommerce.Notification.Infrastructure.SignalR.Facade;
using SmartEcommerce.Notification.Infrastructure.SignalR.Hubs.Global;

namespace SmartEcommerce.Notification.Infrastructure.Delivery;

/// <summary>
/// Pushes a dispatch to the recipient's live SignalR connection(s) via <see cref="GlobalHub"/>.
/// The only channel with a real delivery implementation so far - Email/Telegram/Facebook/Zalo/Push
/// have no provider wired up yet, see <see cref="ChannelSenderResolver"/>.
/// </summary>
public sealed class SignalRChannelSender(
    ActorHubFacade<GlobalHub, IGlobalHubClient, IGlobalHubClient> hub) : IChannelSender
{
    public NotificationChannelType ChannelType => NotificationChannelType.SignalR;

    public async Task SendAsync(NotificationDispatch dispatch, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Deserialize<NotificationDispatchPayload>(dispatch.Payload)
            ?? throw new InvalidOperationException(
                $"Dispatch {dispatch.Id}'s payload is not a valid NotificationDispatchPayload.");

        var dto = new NotificationDto(
            payload.RecipientUserId,
            payload.Category,
            payload.Type,
            payload.Title,
            payload.Content,
            Metadata: "{}",
            NotificationPriority.Normal,
            NotificationStatus.Unread,
            ExpiredAt: DateTime.UtcNow.AddDays(30));

        await hub.Member(payload.RecipientUserId).ReceiveNotification(dto);
    }
}
