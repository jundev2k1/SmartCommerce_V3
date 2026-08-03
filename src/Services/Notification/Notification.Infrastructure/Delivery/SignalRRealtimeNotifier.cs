using SmartEcommerce.Notification.Application.Abstractions.Services;
using SmartEcommerce.Notification.Application.Features.UserNotifications.DTOs;
using SmartEcommerce.Notification.Infrastructure.SignalR.Facade;
using SmartEcommerce.Notification.Infrastructure.SignalR.Hubs.Global;

namespace SmartEcommerce.Notification.Infrastructure.Delivery;

public sealed class SignalRRealtimeNotifier(
    ActorHubFacade<GlobalHub, IGlobalHubClient, IGlobalHubClient> hub) : IRealtimeNotifier
{
    public async Task PushToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
    {
        await hub.Member(userId).ReceiveNotification(notification);
    }

    public async Task PushOrderStatusUpdatedAsync(Guid userId, OrderStatusUpdatedDto notification, CancellationToken ct = default)
    {
        await hub.Member(userId).OrderStatusUpdated(notification);
    }

    public async Task PushNewOrderToAdminsAsync(NewOrderNotificationDto notification, CancellationToken ct = default)
    {
        await hub.AdminAll().OrderCreated(notification);
    }
}
