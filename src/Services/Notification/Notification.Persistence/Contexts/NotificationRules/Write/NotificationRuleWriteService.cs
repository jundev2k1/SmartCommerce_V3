using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationRules;
using Notification.Persistence.Contexts.NotificationRules.Repositories;

namespace Notification.Persistence.Contexts.NotificationRules.Write;

public sealed class NotificationRuleWriteService(
    INotificationRuleRepository repo,
    IUnitOfWork unitOfWork) : INotificationRuleWriteService
{
    public async Task CreateAsync(NotificationRule entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
