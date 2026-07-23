namespace Notification.Persistence.NotificationGroups.Repositories;

public sealed class NotificationGroupRepo(NotificationMongoContext context) : INotificationGroupRepository
{
    public async Task AddAsync(NotificationGroup entity, CancellationToken ct = default)
    {
        await context.NotificationGroups.InsertOneAsync(entity, cancellationToken: ct);
    }
}
