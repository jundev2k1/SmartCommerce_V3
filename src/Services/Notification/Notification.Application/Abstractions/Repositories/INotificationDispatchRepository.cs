namespace Notification.Application.Abstractions.Repositories;

public interface INotificationDispatchRepository
{
    Task<NotificationDispatch?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(NotificationDispatch entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationDispatch entity, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationDispatch> Items, int TotalCount)> SearchAsync(
        DispatchStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Rows a worker should attempt next - Pending, or Failed with NextRetryAt due. Not exposed via API - consumed only by Notification.Infrastructure's dispatch worker.</summary>
    Task<IReadOnlyList<NotificationDispatch>> GetDueForProcessingAsync(int batchSize, CancellationToken ct = default);
}
