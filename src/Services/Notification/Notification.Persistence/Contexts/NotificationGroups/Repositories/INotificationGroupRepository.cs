namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationGroups.Repositories;

public interface INotificationGroupRepository
{
    Task AddAsync(NotificationGroup entity, CancellationToken ct = default);
}
