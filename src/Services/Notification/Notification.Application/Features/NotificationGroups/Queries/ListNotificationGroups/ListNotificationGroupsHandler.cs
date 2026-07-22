using BuildingBlock.Application.Abstractions.Common;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationGroups.Queries.ListNotificationGroups;

public sealed class ListNotificationGroupsHandler(INotificationGroupRepository notificationGroupRepo)
    : IQueryHandler<ListNotificationGroupsQuery, PaginatedResult<NotificationGroupSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationGroupSummaryResponse>> Handle(ListNotificationGroupsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationGroupRepo.SearchAsync(request.Search, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationGroupSummaryResponse(x.Id, x.Name, x.Status, x.Audience.Type, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationGroupSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
