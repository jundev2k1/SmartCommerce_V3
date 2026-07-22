namespace Notification.Application.Abstractions.Repositories;

public interface INotificationRuleRepository
{
    Task<NotificationRule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(NotificationRule entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationRule entity, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationRule> Items, int TotalCount)> SearchAsync(
        string? eventType,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
