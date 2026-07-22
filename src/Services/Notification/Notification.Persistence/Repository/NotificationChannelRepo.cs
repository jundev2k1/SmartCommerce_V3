using Notification.Application.Abstractions.Repositories;

namespace Notification.Persistence.Repository;

public sealed class NotificationChannelRepo(NotificationMongoContext context) : INotificationChannelRepository
{
    public async Task<NotificationChannel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationChannels.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default)
    {
        return await context.NotificationChannels.Find(x => x.ChannelType == channelType).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(NotificationChannel entity, CancellationToken ct = default)
    {
        await context.NotificationChannels.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default)
    {
        await context.NotificationChannels.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<NotificationChannel>> ListAsync(CancellationToken ct = default)
    {
        return await context.NotificationChannels
            .Find(Builders<NotificationChannel>.Filter.Empty)
            .SortBy(x => x.ChannelType)
            .ToListAsync(ct);
    }
}
