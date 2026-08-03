using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationChannels;
using SmartEcommerce.Notification.Persistence.Contexts.NotificationChannels.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationChannels.Write;

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
