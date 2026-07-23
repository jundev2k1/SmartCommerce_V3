namespace Notification.Persistence.NotificationRules.Repositories;

public interface INotificationRuleRepository
{
    Task AddAsync(NotificationRule entity, CancellationToken ct = default);
}
