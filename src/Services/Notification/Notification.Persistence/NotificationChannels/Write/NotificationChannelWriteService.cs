using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationChannels;
using Notification.Persistence.NotificationChannels.Repositories;

namespace Notification.Persistence.NotificationChannels.Write;

public sealed class NotificationChannelWriteService(
    INotificationChannelRepository repo,
    IUnitOfWork unitOfWork) : INotificationChannelWriteService
{
    public async Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default)
    {
        await repo.UpdateAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
