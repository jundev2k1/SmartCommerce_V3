namespace Notification.Application.Abstractions.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(NotificationTemplate entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationTemplate entity, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> SearchAsync(
        NotificationChannelType? channel,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
