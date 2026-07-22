using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationChannels.Queries.ListNotificationChannels;

public sealed class ListNotificationChannelsHandler(INotificationChannelRepository notificationChannelRepo)
    : IQueryHandler<ListNotificationChannelsQuery, IReadOnlyList<NotificationChannelSummaryResponse>>
{
    public async Task<IReadOnlyList<NotificationChannelSummaryResponse>> Handle(ListNotificationChannelsQuery request, CancellationToken ct = default)
    {
        var items = await notificationChannelRepo.ListAsync(ct);

        return [.. items.Select(x => new NotificationChannelSummaryResponse(x.Id, x.ChannelType, x.DisplayName, x.Status, x.ValidationStatus))];
    }
}
