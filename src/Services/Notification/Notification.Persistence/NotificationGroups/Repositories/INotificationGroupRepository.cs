namespace Notification.Persistence.NotificationGroups.Repositories;

public interface INotificationGroupRepository
{
    Task AddAsync(NotificationGroup entity, CancellationToken ct = default);
}
