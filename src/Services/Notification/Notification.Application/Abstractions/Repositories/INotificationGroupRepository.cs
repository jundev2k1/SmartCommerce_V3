namespace Notification.Application.Abstractions.Repositories;

public interface INotificationGroupRepository
{
    Task<NotificationGroup?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(NotificationGroup entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationGroup entity, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationGroup> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
