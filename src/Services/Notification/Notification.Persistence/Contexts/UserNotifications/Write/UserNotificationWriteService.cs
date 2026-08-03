using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.UserNotifications;
using SmartEcommerce.Notification.Persistence.Contexts.UserNotifications.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.UserNotifications.Write;

public sealed class UserNotificationWriteService(
    IUserNotificationRepository repo,
    IUnitOfWork unitOfWork) : IUserNotificationWriteService
{
    public async Task CreateAsync(UserNotification entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UserNotification entity, CancellationToken ct = default)
    {
        await repo.UpdateAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
