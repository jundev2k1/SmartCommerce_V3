using BuildingBlock.Application.Abstractions.Common;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationDispatches.Queries.ListNotificationDispatches;

public sealed class ListNotificationDispatchesHandler(INotificationDispatchRepository notificationDispatchRepo)
    : IQueryHandler<ListNotificationDispatchesQuery, PaginatedResult<NotificationDispatchSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationDispatchSummaryResponse>> Handle(ListNotificationDispatchesQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationDispatchRepo.SearchAsync(request.Status, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationDispatchSummaryResponse(
                x.Id, x.Reference.ReferenceType, x.Reference.ReferenceId, x.Channel, x.Status, x.RetryCount, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationDispatchSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
