using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.UserNotifications.Queries.ListMyUserNotifications;

public sealed class ListMyUserNotificationsHandler(
    ICurrentUserService currentUser,
    IUserNotificationRepository userNotificationRepo) : IQueryHandler<ListMyUserNotificationsQuery, CursorPaginatedResult<UserNotificationSummaryResponse>>
{
    public async Task<CursorPaginatedResult<UserNotificationSummaryResponse>> Handle(ListMyUserNotificationsQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var cursor = UserNotificationCursor.Decode(request.Cursor);

        // Fetch one extra row to know whether a next page exists without a second round trip.
        var items = await userNotificationRepo.GetMineAsync(
            userId, request.Status, cursor?.CreatedAt, cursor?.Id, request.Limit + 1, ct);

        var hasMore = items.Count > request.Limit;
        var page = (hasMore ? items.Take(request.Limit) : items)
            .Select(x => new UserNotificationSummaryResponse(
                x.Id, x.Category.Value, x.Type.Value, x.Content.Title, x.Priority, x.Status, x.CreatedAt))
            .ToList();

        var nextCursor = hasMore
            ? UserNotificationCursor.Encode(page[^1].CreatedAt, page[^1].Id)
            : null;

        return CursorPaginatedResult<UserNotificationSummaryResponse>.Create(page, nextCursor);
    }
}
