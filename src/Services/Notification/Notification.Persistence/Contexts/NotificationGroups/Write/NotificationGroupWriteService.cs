using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationGroups;
using SmartEcommerce.Notification.Persistence.Contexts.NotificationGroups.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationGroups.Write;

public sealed class NotificationGroupWriteService(
    INotificationGroupRepository repo,
    IUnitOfWork unitOfWork) : INotificationGroupWriteService
{
    public async Task CreateAsync(NotificationGroup entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
