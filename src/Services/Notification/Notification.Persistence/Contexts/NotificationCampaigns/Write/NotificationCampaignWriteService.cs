using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;

using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationCampaigns;
using SmartEcommerce.Notification.Persistence.Contexts.NotificationCampaigns.Repositories;

namespace SmartEcommerce.Notification.Persistence.Contexts.NotificationCampaigns.Write;

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
