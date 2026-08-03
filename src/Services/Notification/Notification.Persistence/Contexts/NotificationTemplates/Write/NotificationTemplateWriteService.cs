using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationTemplates;
using SmartEcommerce.Notification.Persistence.Contexts.NotificationTemplates.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationTemplates.Write;

public sealed class NotificationTemplateWriteService(
    INotificationTemplateRepository repo,
    IUnitOfWork unitOfWork) : INotificationTemplateWriteService
{
    public async Task CreateAsync(NotificationTemplate entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
