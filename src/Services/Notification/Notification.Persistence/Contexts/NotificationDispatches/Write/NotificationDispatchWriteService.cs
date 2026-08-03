using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationDispatches;
using SmartEcommerce.Notification.Persistence.Contexts.NotificationDispatches.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationDispatches.Write;

public sealed class NotificationDispatchWriteService(
    INotificationDispatchRepository repo,
    IUnitOfWork unitOfWork) : INotificationDispatchWriteService
{
    public async Task CreateAsync(NotificationDispatch entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(NotificationDispatch entity, CancellationToken ct = default)
    {
        await repo.UpdateAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
