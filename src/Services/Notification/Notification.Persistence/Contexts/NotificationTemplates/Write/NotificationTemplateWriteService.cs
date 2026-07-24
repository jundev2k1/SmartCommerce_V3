using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationTemplates;
using Notification.Persistence.Contexts.NotificationTemplates.Repositories;

namespace Notification.Persistence.Contexts.NotificationTemplates.Write;

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
