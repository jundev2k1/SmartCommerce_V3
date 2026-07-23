using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationDispatches;
using Notification.Persistence.NotificationDispatches.Repositories;

namespace Notification.Persistence.NotificationDispatches.Write;

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
