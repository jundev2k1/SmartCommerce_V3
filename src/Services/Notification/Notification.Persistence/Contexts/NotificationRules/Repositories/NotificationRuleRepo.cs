using Notification.Persistence.Engine;

namespace Notification.Persistence.Contexts.NotificationRules.Repositories;

public sealed class NotificationRuleRepo(NotificationMongoContext context) : INotificationRuleRepository
{
    public async Task AddAsync(NotificationRule entity, CancellationToken ct = default)
    {
        await context.NotificationRules.InsertOneAsync(entity, cancellationToken: ct);
    }
}
