namespace Notification.Application.Abstractions.Repositories;

/// <summary>Channels are few (one row per NotificationChannelType) and never paginated - ListAsync returns the full set, same reasoning a lookup/reference table would.</summary>
public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default);

    Task AddAsync(NotificationChannel entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default);

    Task<IReadOnlyList<NotificationChannel>> ListAsync(CancellationToken ct = default);
}
