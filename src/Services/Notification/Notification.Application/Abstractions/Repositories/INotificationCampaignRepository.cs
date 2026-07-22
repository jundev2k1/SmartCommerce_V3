namespace Notification.Application.Abstractions.Repositories;

public interface INotificationCampaignRepository
{
    Task<NotificationCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(NotificationCampaign entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationCampaign entity, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationCampaign> Items, int TotalCount)> SearchAsync(
        CampaignStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
