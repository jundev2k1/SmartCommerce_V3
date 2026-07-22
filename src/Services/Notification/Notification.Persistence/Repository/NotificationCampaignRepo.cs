using Notification.Application.Abstractions.Repositories;

namespace Notification.Persistence.Repository;

public sealed class NotificationCampaignRepo(NotificationMongoContext context) : INotificationCampaignRepository
{
    public async Task<NotificationCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationCampaigns.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(NotificationCampaign entity, CancellationToken ct = default)
    {
        await context.NotificationCampaigns.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task UpdateAsync(NotificationCampaign entity, CancellationToken ct = default)
    {
        await context.NotificationCampaigns.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }

    public async Task<(IReadOnlyList<NotificationCampaign> Items, int TotalCount)> SearchAsync(
        CampaignStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationCampaign>.Filter;
        var filter = filterBuilder.Empty;

        if (status is not null)
            filter &= filterBuilder.Eq(x => x.Status, status.Value);

        var totalCount = (int)await context.NotificationCampaigns.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationCampaigns
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
