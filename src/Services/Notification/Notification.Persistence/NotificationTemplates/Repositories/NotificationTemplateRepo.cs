namespace Notification.Persistence.NotificationTemplates.Repositories;

public sealed class NotificationTemplateRepo(NotificationMongoContext context) : INotificationTemplateRepository
{
    public async Task AddAsync(NotificationTemplate entity, CancellationToken ct = default)
    {
        await context.NotificationTemplates.InsertOneAsync(entity, cancellationToken: ct);
    }
}
