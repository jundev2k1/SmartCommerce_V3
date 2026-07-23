namespace Notification.Persistence.NotificationChannels.Repositories;

public interface INotificationChannelRepository
{
    Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default);
}
