using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;

namespace SmartEcommerce.Notification.Application.Features.UserNotifications.Queries.ListMyUserNotifications;

public sealed record ListMyUserNotificationsQuery(
    NotificationStatus? Status,
    string? Cursor,
    int Limit = 20) : IQuery<CursorPaginatedResult<UserNotificationSummaryResponse>>;

public sealed record UserNotificationSummaryResponse(
    Guid Id,
    string Category,
    string Type,
    string Title,
    NotificationPriority Priority,
    NotificationStatus Status,
    DateTime CreatedAt);
