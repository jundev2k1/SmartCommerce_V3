using SmartEcommerce.Notification.Persistence.Engine;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationTemplates.Repositories;

public sealed class NotificationTemplateRepo(NotificationMongoContext context) : INotificationTemplateRepository
{
    public async Task AddAsync(NotificationTemplate entity, CancellationToken ct = default)
    {
        await context.NotificationTemplates.InsertOneAsync(entity, cancellationToken: ct);
    }
}
