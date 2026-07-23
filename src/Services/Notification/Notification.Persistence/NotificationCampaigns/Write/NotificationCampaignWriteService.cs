using BuildingBlock.Application.Abstractions.Persistence;

using Notification.Application.Abstractions.Persistence.NotificationCampaigns;
using Notification.Persistence.NotificationCampaigns.Repositories;

namespace Notification.Persistence.NotificationCampaigns.Write;

public sealed class NotificationCampaignWriteService(
    INotificationCampaignRepository repo,
    IUnitOfWork unitOfWork) : INotificationCampaignWriteService
{
    public async Task CreateAsync(NotificationCampaign entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
