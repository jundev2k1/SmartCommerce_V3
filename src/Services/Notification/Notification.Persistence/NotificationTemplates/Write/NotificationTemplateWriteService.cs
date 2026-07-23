using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationTemplates;
using Notification.Persistence.NotificationTemplates.Repositories;

namespace Notification.Persistence.NotificationTemplates.Write;

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
